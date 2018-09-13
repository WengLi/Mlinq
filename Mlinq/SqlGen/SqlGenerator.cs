using Mlinq.Common;
using Mlinq.Core.LinqPredicate;
using Mlinq.Core.LinqPredicate.DbCommandTrees;
using Mlinq.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.SqlGen
{
    internal class SqlGenerator : PredicateVisitor<ISqlFragment>
    {
        private Stack<SqlSelectStatement> selectStatementStack;

        private SqlSelectStatement CurrentSelectStatement
        {
            get { return selectStatementStack.Peek(); }
        }

        private Stack<bool> isParentAJoinStack;

        private bool IsParentAJoin
        {
            get { return isParentAJoinStack.Count != 0 && isParentAJoinStack.Peek(); }
        }

        private Dictionary<string, int> allExtentNames;

        internal Dictionary<string, int> AllExtentNames
        {
            get { return allExtentNames; }
        }

        private Dictionary<string, int> allColumnNames;

        internal Dictionary<string, int> AllColumnNames
        {
            get { return allColumnNames; }
        }

        private bool IsPreKatmai = false;

        private readonly SymbolTable symbolTable = new SymbolTable();

        private bool isVarRefSingle;

        private readonly SymbolUsageManager optionalColumnUsageManager = new SymbolUsageManager();

        private readonly Dictionary<string, bool> _candidateParametersToForceNonUnicode = new Dictionary<string, bool>();

        private bool _forceNonUnicode;

        private bool _ignoreForceNonUnicodeFlag;

        private const byte DefaultDecimalPrecision = 18;
        private static readonly char[] _hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        private List<string> _targets;

        public List<string> Targets
        {
            get { return _targets; }
        }

        private static readonly ISet<string> _canonicalAndStoreStringFunctionsOneArg =
            new HashSet<string>(StringComparer.Ordinal)
                {
                    "Edm.Trim",
                    "Edm.RTrim",
                    "Edm.LTrim",
                    "Edm.Left",
                    "Edm.Right",
                    "Edm.Substring",
                    "Edm.ToLower",
                    "Edm.ToUpper",
                    "Edm.Reverse",
                    "SqlServer.RTRIM",
                    "SqlServer.LTRIM",
                    "SqlServer.LEFT",
                    "SqlServer.RIGHT",
                    "SqlServer.SUBSTRING",
                    "SqlServer.LOWER",
                    "SqlServer.UPPER",
                    "SqlServer.REVERSE"
                };

        private TypeUsage _integerType;

        internal TypeUsage IntegerType
        {
            get
            {
                return null;
            }
        }

        internal SqlGenerator()
        {
        }

        internal static string GenerateSql(DbCommandTree tree, out List<SqlParameter> parameters, out CommandType commandType, out HashSet<string> paramsToForceNonUnicode)
        {
            commandType = CommandType.Text;
            parameters = null;
            paramsToForceNonUnicode = null;

            var sqlGenerator = new SqlGenerator();

            switch (tree.CommandTreeKind)
            {
                case DbCommandTreeKind.Query:
                    return sqlGenerator.GenerateSql((DbQueryCommandTree)tree, out paramsToForceNonUnicode);
                default:
                    return null;
            }
        }

        internal string GenerateSql(DbQueryCommandTree tree, out HashSet<string> paramsToForceNonUnicode)
        {
            _targets = new List<string>();

            var targetTree = tree;

            selectStatementStack = new Stack<SqlSelectStatement>();
            isParentAJoinStack = new Stack<bool>();

            allExtentNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            allColumnNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);


            ISqlFragment result;

            if (BuiltInTypeKind.CollectionType == targetTree.Query.ResultType.EdmType.BuiltInTypeKind)
            {
                var sqlStatement = VisitExpressionEnsureSqlStatement(targetTree.Query);
                sqlStatement.IsTopMost = true;
                result = sqlStatement;
            }
            else
            {
                var sqlBuilder = new SqlBuilder();

                sqlBuilder.Append("SELECT ");
                sqlBuilder.Append(targetTree.Query.Accept(this));

                result = sqlBuilder;
            }

            if (isVarRefSingle)
            {
                throw new NotSupportedException();
            }
            paramsToForceNonUnicode = new HashSet<string>(_candidateParametersToForceNonUnicode.Where(p => p.Value).Select(q => q.Key).ToList());

            var builder = new StringBuilder(1024);
            var writer = new SqlWriter(builder);
            WriteSql(writer, result);
            return builder.ToString();
        }

        internal SqlWriter WriteSql(SqlWriter writer, ISqlFragment sqlStatement)
        {
            sqlStatement.WriteSql(writer, this);
            return writer;
        }

        private SqlSelectStatement VisitExpressionEnsureSqlStatement(Predicate e)
        {
            return VisitExpressionEnsureSqlStatement(e, true, false);
        }

        private SqlSelectStatement VisitExpressionEnsureSqlStatement(Predicate e, bool addDefaultColumns, bool markAllDefaultColumnsAsUsed)
        {
            SqlSelectStatement result;
            switch (e.PredicateType)
            {
                case PredicateType.Project:
                case PredicateType.Filter:
                case PredicateType.GroupBy:
                case PredicateType.Sort:
                    result = e.Accept(this) as SqlSelectStatement;
                    break;

                default:
                    Symbol fromSymbol;
                    var inputVarName = "c";
                    symbolTable.EnterScope();

                    TypeUsage type = null;
                    switch (e.PredicateType)
                    {
                        case PredicateType.Scan:
                        case PredicateType.CrossJoin:
                        case PredicateType.FullOuterJoin:
                        case PredicateType.InnerJoin:
                        case PredicateType.LeftOuterJoin:
                        case PredicateType.CrossApply:
                        case PredicateType.OuterApply:
                            type = e.ResultType.GetElementTypeUsage();
                            break;

                        default:
                            type = TypeUsage.Create(e.ResultType.EdmType);
                            break;
                    }

                    result = VisitInputExpression(e, inputVarName, type, out fromSymbol);
                    AddFromSymbol(result, inputVarName, fromSymbol);
                    symbolTable.ExitScope();
                    break;
            }

            if (addDefaultColumns && result.Select.IsEmpty)
            {
                var defaultColumns = AddDefaultColumns(result);
                if (markAllDefaultColumnsAsUsed)
                {
                    foreach (var symbol in defaultColumns)
                    {
                        optionalColumnUsageManager.MarkAsUsed(symbol);
                    }
                }
            }

            return result;
        }

        private SqlSelectStatement VisitInputExpression(Predicate inputPredicate, string inputVarName, TypeUsage inputVarType, out Symbol fromSymbol)
        {
            SqlSelectStatement result;
            var sqlFragment = inputPredicate.Accept(this);
            result = sqlFragment as SqlSelectStatement;

            if (result == null)
            {
                result = new SqlSelectStatement();
                WrapNonQueryExtent(result, sqlFragment, inputPredicate.PredicateType);
            }

            if (result.FromExtents.Count == 0)
            {
                fromSymbol = new Symbol(inputVarName, inputVarType);
            }
            else if (result.FromExtents.Count == 1)
            {
                fromSymbol = result.FromExtents[0];
            }
            else
            {
                var joinSymbol = new JoinSymbol(inputVarName, inputVarType, result.FromExtents);
                joinSymbol.FlattenedExtentList = result.AllJoinExtents;

                fromSymbol = joinSymbol;
                result.FromExtents.Clear();
                result.FromExtents.Add(fromSymbol);
            }

            return result;
        }

        private static void WrapNonQueryExtent(SqlSelectStatement result, ISqlFragment sqlFragment, PredicateType predicateType)
        {
            switch (predicateType)
            {
                case PredicateType.Function:
                    result.From.Append(sqlFragment);
                    break;
                default:
                    result.From.Append(" (");
                    result.From.Append(sqlFragment);
                    result.From.Append(")");
                    break;
            }
        }

        private void AddFromSymbol(SqlSelectStatement selectStatement, string inputVarName, Symbol fromSymbol)
        {
            AddFromSymbol(selectStatement, inputVarName, fromSymbol, true);
        }

        private void AddFromSymbol(SqlSelectStatement selectStatement, string inputVarName, Symbol fromSymbol, bool addToSymbolTable)
        {
            if (selectStatement.FromExtents.Count == 0 || fromSymbol != selectStatement.FromExtents[0])
            {
                selectStatement.FromExtents.Add(fromSymbol);
                selectStatement.From.Append(" AS ");
                selectStatement.From.Append(fromSymbol);
                allExtentNames[fromSymbol.Name] = 0;
            }

            if (addToSymbolTable)
            {
                symbolTable.Add(inputVarName, fromSymbol);
            }
        }

        private List<Symbol> AddDefaultColumns(SqlSelectStatement selectStatement)
        {
            var columnList = new List<Symbol>();
            var columnDictionary = new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase);

            foreach (var symbol in selectStatement.FromExtents)
            {
                AddColumns(selectStatement, symbol, columnList, columnDictionary);
            }

            return columnList;
        }

        private void AddColumns(SqlSelectStatement selectStatement, Symbol symbol, List<Symbol> columnList, Dictionary<string, Symbol> columnDictionary)
        {
            var joinSymbol = symbol as JoinSymbol;
            if (joinSymbol != null)
            {
                if (!joinSymbol.IsNestedJoin)
                {
                    foreach (var sym in joinSymbol.ExtentList)
                    {
                        if ((sym.Type == null) || BuiltInTypeKind.PrimitiveType == sym.Type.EdmType.BuiltInTypeKind)
                        {
                            continue;
                        }

                        AddColumns(selectStatement, sym, columnList, columnDictionary);
                    }
                }
                else
                {
                    foreach (var joinColumn in joinSymbol.ColumnList)
                    {
                        var optionalColumn = CreateOptionalColumn(null, joinColumn);

                        optionalColumn.Append(symbol);
                        optionalColumn.Append(".");
                        optionalColumn.Append(joinColumn);

                        selectStatement.Select.AddOptionalColumn(optionalColumn);

                        if (columnDictionary.ContainsKey(joinColumn.Name))
                        {
                            columnDictionary[joinColumn.Name].NeedsRenaming = true;
                            joinColumn.NeedsRenaming = true;
                        }
                        else
                        {
                            columnDictionary[joinColumn.Name] = joinColumn;
                        }

                        columnList.Add(joinColumn);
                    }
                }
            }
            else
            {
                if (symbol.OutputColumnsRenamed)
                {
                    selectStatement.OutputColumnsRenamed = true;
                }

                if (selectStatement.OutputColumns == null)
                {
                    selectStatement.OutputColumns = new Dictionary<string, Symbol>();
                }

                if ((symbol.Type == null) || BuiltInTypeKind.PrimitiveType == symbol.Type.EdmType.BuiltInTypeKind)
                {
                    AddColumn(selectStatement, symbol, columnList, columnDictionary, "X");
                }
                else
                {
                    foreach (var property in symbol.Type.GetProperties())
                    {
                        AddColumn(selectStatement, symbol, columnList, columnDictionary, property.Name);
                    }
                }
            }
        }

        private void AddColumn(SqlSelectStatement selectStatement, Symbol symbol, List<Symbol> columnList, Dictionary<string, Symbol> columnDictionary, string columnName)
        {
            allColumnNames[columnName] = 0;
            Symbol inputSymbol = null;
            symbol.OutputColumns.TryGetValue(columnName, out inputSymbol);
            Symbol columnSymbol;
            if (!symbol.Columns.TryGetValue(columnName, out columnSymbol))
            {
                columnSymbol = ((inputSymbol != null) && symbol.OutputColumnsRenamed) ? inputSymbol : new Symbol(columnName, null);
                symbol.Columns.Add(columnName, columnSymbol);
            }

            var optionalColumn = CreateOptionalColumn(inputSymbol, columnSymbol);

            optionalColumn.Append(symbol);
            optionalColumn.Append(".");

            if (symbol.OutputColumnsRenamed)
            {
                optionalColumn.Append(inputSymbol);
            }
            else
            {
                optionalColumn.Append(QuoteIdentifier(columnName));
            }

            optionalColumn.Append(" AS ");
            optionalColumn.Append(columnSymbol);

            selectStatement.Select.AddOptionalColumn(optionalColumn);

            if (!selectStatement.OutputColumns.ContainsKey(columnName))
            {
                selectStatement.OutputColumns.Add(columnName, columnSymbol);
            }

            if (columnDictionary.ContainsKey(columnName))
            {
                columnDictionary[columnName].NeedsRenaming = true;
                columnSymbol.NeedsRenaming = true;
            }
            else
            {
                columnDictionary[columnName] = symbol.Columns[columnName];
            }

            columnList.Add(columnSymbol);
        }

        private OptionalColumn CreateOptionalColumn(Symbol inputColumnSymbol, Symbol column)
        {
            if (!optionalColumnUsageManager.ContainsKey(column))
            {
                optionalColumnUsageManager.Add(inputColumnSymbol, column);
            }
            return new OptionalColumn(optionalColumnUsageManager, column);
        }

        private SqlBuilder VisitComparisonExpression(string op, Predicate left, Predicate right)
        {
            RemoveUnnecessaryCasts(ref left, ref right);

            var result = new SqlBuilder();

            var isCastOptional = left.ResultType.EdmType == right.ResultType.EdmType;

            if (left.PredicateType == PredicateType.Constant)
            {
                result.Append(VisitConstant((ConstantPredicate)left, isCastOptional));
            }
            else
            {
                ParenthesizeExpressionIfNeeded(left, result);
            }

            result.Append(op);

            if (right.PredicateType == PredicateType.Constant)
            {
                result.Append(VisitConstant((ConstantPredicate)right, isCastOptional));
            }
            else
            {
                ParenthesizeExpressionIfNeeded(right, result);
            }

            return result;
        }

        private void ParenthesizeExpressionIfNeeded(Predicate e, SqlBuilder result)
        {
            if (IsComplexExpression(e))
            {
                result.Append("(");
                result.Append(e.Accept(this));
                result.Append(")");
            }
            else
            {
                result.Append(e.Accept(this));
            }
        }

        private static bool IsComplexExpression(Predicate e)
        {
            switch (e.PredicateType)
            {
                case PredicateType.Constant:
                case PredicateType.ParameterReference:
                case PredicateType.Property:
                case PredicateType.Cast:
                    return false;

                default:
                    return true;
            }
        }

        private ISqlFragment VisitConstant(ConstantPredicate e, bool isCastOptional)
        {
            var result = new SqlBuilder();

            var resultType = e.ResultType;
            if (resultType.IsPrimitiveType())
            {
                var typeKind = resultType.GetPrimitiveTypeKind();
                switch (typeKind)
                {
                    case PrimitiveTypeKind.Int32:
                        result.Append(e.Value.ToString());
                        break;

                    case PrimitiveTypeKind.Binary:
                        result.Append(" 0x");
                        result.Append(ByteArrayToBinaryString((Byte[])e.Value));
                        result.Append(" ");
                        break;

                    case PrimitiveTypeKind.Boolean:
                        WrapWithCastIfNeeded(!isCastOptional, (bool)e.Value ? "1" : "0", "bit", result);
                        break;
                    case PrimitiveTypeKind.Byte:
                        WrapWithCastIfNeeded(!isCastOptional, e.Value.ToString(), "tinyint", result);
                        break;
                    case PrimitiveTypeKind.DateTime:
                        result.Append("convert(");
                        result.Append(IsPreKatmai ? "datetime" : "datetime2");
                        result.Append(", ");
                        result.Append(EscapeSingleQuote(((DateTime)e.Value).ToString(IsPreKatmai ? "yyyy-MM-dd HH:mm:ss.fff" : "yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture), false));
                        result.Append(", 121)");
                        break;

                    case PrimitiveTypeKind.Time:
                        result.Append("convert(");
                        result.Append(e.ResultType.EdmType.Name);
                        result.Append(", ");
                        result.Append(EscapeSingleQuote(e.Value.ToString(), false /* IsUnicode */));
                        result.Append(", 121)");
                        break;

                    case PrimitiveTypeKind.DateTimeOffset:
                        result.Append("convert(");
                        result.Append(e.ResultType.EdmType.Name);
                        result.Append(", ");
                        result.Append(EscapeSingleQuote(((DateTimeOffset)e.Value).ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz", CultureInfo.InvariantCulture), false));
                        result.Append(", 121)");
                        break;

                    case PrimitiveTypeKind.Decimal:
                        var strDecimal = ((Decimal)e.Value).ToString(CultureInfo.InvariantCulture);
                        var needsCast = -1 == strDecimal.IndexOf('.') && (strDecimal.TrimStart(new[] { '-' }).Length < 20);

                        var precision = Math.Max((Byte)strDecimal.Length, DefaultDecimalPrecision);

                        var decimalType = "decimal(" + precision.ToString(CultureInfo.InvariantCulture) + ")";

                        WrapWithCastIfNeeded(needsCast, strDecimal, decimalType, result);
                        break;

                    case PrimitiveTypeKind.Double:
                        {
                            var doubleValue = (Double)e.Value;
                            AssertValidDouble(doubleValue);
                            WrapWithCastIfNeeded(true, doubleValue.ToString("R", CultureInfo.InvariantCulture), "float(53)", result);
                        }
                        break;

                    case PrimitiveTypeKind.Guid:
                        WrapWithCastIfNeeded(true, EscapeSingleQuote(e.Value.ToString(), false /* IsUnicode */), "uniqueidentifier", result);
                        break;

                    case PrimitiveTypeKind.Int16:
                        WrapWithCastIfNeeded(!isCastOptional, e.Value.ToString(), "smallint", result);
                        break;

                    case PrimitiveTypeKind.Int64:
                        WrapWithCastIfNeeded(!isCastOptional, e.Value.ToString(), "bigint", result);
                        break;

                    case PrimitiveTypeKind.Single:
                        {
                            var singleValue = (float)e.Value;
                            AssertValidSingle(singleValue);
                            WrapWithCastIfNeeded(true, singleValue.ToString("R", CultureInfo.InvariantCulture), "real", result);
                        }
                        break;

                    case PrimitiveTypeKind.String:
                        bool isUnicode;

                        if (!e.ResultType.TryGetIsUnicode(out isUnicode))
                        {
                            isUnicode = !_forceNonUnicode;
                        }
                        result.Append(EscapeSingleQuote(e.Value as string, isUnicode));
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            return result;
        }

        private static void AssertValidSingle(float value)
        {
            if (float.IsNaN(value))
            {
                throw new NotSupportedException();
            }
            else if (float.IsPositiveInfinity(value))
            {
                throw new NotSupportedException();
            }
            else if (float.IsNegativeInfinity(value))
            {
                throw new NotSupportedException();
            }
        }

        private static void AssertValidDouble(double value)
        {
            if (double.IsNaN(value))
            {
                throw new NotSupportedException();
            }
            else if (double.IsPositiveInfinity(value))
            {
                throw new NotSupportedException();
            }
            else if (double.IsNegativeInfinity(value))
            {
                throw new NotSupportedException();
            }
        }

        private static void WrapWithCastIfNeeded(bool cast, string value, string typeName, SqlBuilder result)
        {
            if (!cast)
            {
                result.Append(value);
            }
            else
            {
                result.Append("cast(");
                result.Append(value);
                result.Append(" as ");
                result.Append(typeName);
                result.Append(")");
            }
        }

        private static string ByteArrayToBinaryString(Byte[] binaryArray)
        {
            var sb = new StringBuilder(binaryArray.Length * 2);
            for (var i = 0; i < binaryArray.Length; i++)
            {
                sb.Append(_hexDigits[(binaryArray[i] & 0xF0) >> 4]).Append(_hexDigits[binaryArray[i] & 0x0F]);
            }
            return sb.ToString();
        }

        private static void RemoveUnnecessaryCasts(ref Predicate left, ref Predicate right)
        {
            if (left.ResultType.EdmType != right.ResultType.EdmType)
            {
                return;
            }

            var leftCast = left as CastPredicate;
            if (leftCast != null && leftCast.Argument.ResultType.EdmType == left.ResultType.EdmType)
            {
                left = leftCast.Argument;
            }

            var rightCast = right as CastPredicate;
            if (rightCast != null && rightCast.Argument.ResultType.EdmType == left.ResultType.EdmType)
            {
                right = rightCast.Argument;
            }
        }

        private SqlSelectStatement VisitFilterExpression(PredicateBinding input, Predicate predicate, bool negatePredicate)
        {
            Symbol fromSymbol;
            var result = VisitInputExpression(input.Predicate, input.VariableName, input.VariableType, out fromSymbol);

            if (!IsCompatible(result, PredicateType.Filter))
            {
                result = CreateNewSelectStatement(result, input.VariableName, input.VariableType, out fromSymbol);
            }

            selectStatementStack.Push(result);
            symbolTable.EnterScope();

            AddFromSymbol(result, input.VariableName, fromSymbol);

            if (negatePredicate)
            {
                result.Where.Append("NOT (");
            }
            result.Where.Append(predicate.Accept(this));
            if (negatePredicate)
            {
                result.Where.Append(")");
            }

            symbolTable.ExitScope();
            selectStatementStack.Pop();

            return result;
        }

        private SqlSelectStatement CreateNewSelectStatement(SqlSelectStatement oldStatement, string inputVarName, TypeUsage inputVarType, out Symbol fromSymbol)
        {
            return CreateNewSelectStatement(oldStatement, inputVarName, inputVarType, true, out fromSymbol);
        }

        private SqlSelectStatement CreateNewSelectStatement(SqlSelectStatement oldStatement, string inputVarName, TypeUsage inputVarType, bool finalizeOldStatement, out Symbol fromSymbol)
        {
            fromSymbol = null;
            if (finalizeOldStatement && oldStatement.Select.IsEmpty)
            {
                var columns = AddDefaultColumns(oldStatement);

                var oldJoinSymbol = oldStatement.FromExtents[0] as JoinSymbol;
                if (oldJoinSymbol != null)
                {
                    var newJoinSymbol = new JoinSymbol(inputVarName, inputVarType, oldJoinSymbol.ExtentList);
                    newJoinSymbol.IsNestedJoin = true;
                    newJoinSymbol.ColumnList = columns;
                    newJoinSymbol.FlattenedExtentList = oldJoinSymbol.FlattenedExtentList;
                    fromSymbol = newJoinSymbol;
                }
            }

            if (fromSymbol == null)
            {
                fromSymbol = new Symbol(inputVarName, inputVarType, oldStatement.OutputColumns, oldStatement.OutputColumnsRenamed);
            }

            var selectStatement = new SqlSelectStatement();
            selectStatement.From.Append("( ");
            selectStatement.From.Append(oldStatement);
            selectStatement.From.AppendLine();
            selectStatement.From.Append(") ");

            return selectStatement;
        }

        private static bool IsCompatible(SqlSelectStatement result, PredicateType predicateType)
        {
            switch (predicateType)
            {
                case PredicateType.Distinct:
                    return result.Select.Top == null
                           && result.Select.Skip == null
                           && result.OrderBy.IsEmpty;

                case PredicateType.Filter:
                    return result.Select.IsEmpty
                           && result.Where.IsEmpty
                           && result.GroupBy.IsEmpty
                           && result.Select.Top == null
                           && result.Select.Skip == null;

                case PredicateType.GroupBy:
                    return result.Select.IsEmpty
                           && result.GroupBy.IsEmpty
                           && result.OrderBy.IsEmpty
                           && result.Select.Top == null
                           && result.Select.Skip == null
                           && !result.Select.IsDistinct;

                case PredicateType.Limit:
                case PredicateType.Element:
                    return result.Select.Top == null;

                case PredicateType.Project:
                    return result.Select.IsEmpty
                           && result.GroupBy.IsEmpty
                           && !result.Select.IsDistinct;

                case PredicateType.Skip:
                    return result.Select.IsEmpty
                           && result.Select.Skip == null
                           && result.GroupBy.IsEmpty
                           && result.OrderBy.IsEmpty
                           && !result.Select.IsDistinct;

                case PredicateType.Sort:
                    return result.Select.IsEmpty
                           && result.GroupBy.IsEmpty
                           && result.OrderBy.IsEmpty
                           && !result.Select.IsDistinct;

                default:
                    throw new InvalidOperationException(String.Empty);
            }
        }

        private ISqlFragment VisitCollectionConstructor(NewInstancePredicate e)
        {
            if (e.Arguments.Count == 1 && e.Arguments[0].PredicateType == PredicateType.Element)
            {
                var elementExpr = e.Arguments[0] as ElementPredicate;
                var result = VisitExpressionEnsureSqlStatement(elementExpr.Argument);

                if (!IsCompatible(result, PredicateType.Element))
                {
                    Symbol fromSymbol;
                    var inputType = elementExpr.Argument.ResultType.GetElementTypeUsage();

                    result = CreateNewSelectStatement(result, "element", inputType, out fromSymbol);
                    AddFromSymbol(result, "element", fromSymbol, false);
                }
                result.Select.Top = new TopClause(1, false);
                return result;
            }

            var collectionType = (CollectionType)e.ResultType.EdmType;
            var isScalarElement = BuiltInTypeKind.PrimitiveType == collectionType.TypeUsage.EdmType.BuiltInTypeKind;

            var resultSql = new SqlBuilder();
            var separator = "";

            if (e.Arguments.Count == 0)
            {
                //resultSql.Append(" SELECT CAST(null as ");
                //resultSql.Append(GetSqlPrimitiveType(collectionType.TypeUsage));
                //resultSql.Append(") AS X FROM (SELECT 1) AS Y WHERE 1=0");
            }

            foreach (var arg in e.Arguments)
            {
                resultSql.Append(separator);
                resultSql.Append(" SELECT ");
                resultSql.Append(arg.Accept(this));
                if (isScalarElement)
                {
                    resultSql.Append(" AS X ");
                }
                separator = " UNION ALL ";
            }

            return resultSql;
        }

        private ISqlFragment VisitNewInstanceExpression(NewInstancePredicate e, bool aliasesNeedRenaming, out Dictionary<string, Symbol> newColumns)
        {
            var result = new SqlBuilder();
            var rowType = e.ResultType.EdmType as RowType;

            if (null != rowType)
            {
                newColumns = new Dictionary<string, Symbol>(e.Arguments.Count);

                var members = rowType.Properties;
                var separator = "";
                for (var i = 0; i < e.Arguments.Count; ++i)
                {
                    var argument = e.Arguments[i];
                    if (BuiltInTypeKind.RowType == argument.ResultType.EdmType.BuiltInTypeKind)
                    {
                        throw new NotSupportedException();
                    }

                    var member = members[i];
                    result.Append(separator);
                    result.AppendLine();
                    result.Append(argument.Accept(this));
                    result.Append(" AS ");
                    if (aliasesNeedRenaming)
                    {
                        var column = new Symbol(member.Name, member.TypeUsage);
                        column.NeedsRenaming = true;
                        column.NewName = String.Concat("Internal_", member.Name);
                        result.Append(column);
                        newColumns.Add(member.Name, column);
                    }
                    else
                    {
                        result.Append(QuoteIdentifier(member.Name));
                    }
                    separator = ", ";
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            return result;
        }

        internal static string GetTargetTSql(EntitySet entitySet)
        {
            var builder = new StringBuilder(50);

            var schema = entitySet.Schema;
            if (!string.IsNullOrEmpty(schema))
            {
                builder.Append(QuoteIdentifier(schema));
                builder.Append(".");
            }
            else
            {
                builder.Append(QuoteIdentifier(entitySet.EntityContainer.Name));
                builder.Append(".");
            }

            var table = entitySet.Table;
            builder.Append(entitySet.Table);

            return builder.ToString();
        }

        private static string EscapeSingleQuote(string s, bool isUnicode)
        {
            return (isUnicode ? "N'" : "'") + s.Replace("'", "''") + "'";
        }

        internal static string QuoteIdentifier(string name)
        {
            return "[" + name.Replace("]", "]]") + "]";
        }

        #region override

        public override ISqlFragment Visit(Predicate e)
        {

            Check.NotNull(e, "e");

            throw new InvalidOperationException(String.Empty);
        }

        public override ISqlFragment Visit(ComparisonPredicate e)
        {
            Check.NotNull(e, "e");

            SqlBuilder result;

            switch (e.PredicateType)
            {
                case PredicateType.Equals:
                    result = VisitComparisonExpression(" = ", e.Left, e.Right);
                    break;
                case PredicateType.LessThan:
                    result = VisitComparisonExpression(" < ", e.Left, e.Right);
                    break;
                case PredicateType.LessThanOrEquals:
                    result = VisitComparisonExpression(" <= ", e.Left, e.Right);
                    break;
                case PredicateType.GreaterThan:
                    result = VisitComparisonExpression(" > ", e.Left, e.Right);
                    break;
                case PredicateType.GreaterThanOrEquals:
                    result = VisitComparisonExpression(" >= ", e.Left, e.Right);
                    break;
                case PredicateType.NotEquals:
                    result = VisitComparisonExpression(" <> ", e.Left, e.Right);
                    break;

                default:
                    throw new InvalidOperationException(String.Empty);
            }

            _forceNonUnicode = false;

            return result;
        }

        public override ISqlFragment Visit(ConstantPredicate e)
        {
            Check.NotNull(e, "e");

            return VisitConstant(e, false /* isCastOptional */);
        }

        public override ISqlFragment Visit(FilterPredicate e)
        {
            Check.NotNull(e, "e");

            return VisitFilterExpression(e.Input, e.Predicate, false);
        }

        public override ISqlFragment Visit(NewInstancePredicate e)
        {
            Check.NotNull(e, "e");

            if (BuiltInTypeKind.CollectionType == e.ResultType.EdmType.BuiltInTypeKind)
            {
                return VisitCollectionConstructor(e);
            }
            throw new NotSupportedException();
        }

        public override ISqlFragment Visit(ProjectPredicate e)
        {
            Check.NotNull(e, "e");

            Symbol fromSymbol;
            var result = VisitInputExpression(e.Input.Predicate, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
            var aliasesNeedRenaming = false;

            if (!IsCompatible(result, e.PredicateType))
            {
                result = CreateNewSelectStatement(result, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
            }

            selectStatementStack.Push(result);
            symbolTable.EnterScope();

            AddFromSymbol(result, e.Input.VariableName, fromSymbol);

            var newInstanceExpression = e.Projection as NewInstancePredicate;
            if (newInstanceExpression != null)
            {
                Dictionary<string, Symbol> newColumns;
                result.Select.Append(VisitNewInstanceExpression(newInstanceExpression, aliasesNeedRenaming, out newColumns));
                if (aliasesNeedRenaming)
                {
                    result.OutputColumnsRenamed = true;
                }
                result.OutputColumns = newColumns;
            }
            else
            {
                result.Select.Append(e.Projection.Accept(this));
            }

            symbolTable.ExitScope();
            selectStatementStack.Pop();

            return result;
        }

        public override ISqlFragment Visit(PropertyPredicate e)
        {
            Check.NotNull(e, "e");
            SqlBuilder result;
            var instanceSql = e.Instance.Accept(this);

            var VariableReferenceExpression = e.Instance as VariableReferencePredicate;
            if (VariableReferenceExpression != null)
            {
                isVarRefSingle = false;
            }

            var joinSymbol = instanceSql as JoinSymbol;
            if (joinSymbol != null)
            {
                if (joinSymbol.IsNestedJoin)
                {
                    return new SymbolPair(joinSymbol, joinSymbol.NameToExtent[e.Property.Name]);
                }
                else
                {
                    return joinSymbol.NameToExtent[e.Property.Name];
                }
            }

            var symbolPair = instanceSql as SymbolPair;
            if (symbolPair != null)
            {
                var columnJoinSymbol = symbolPair.Column as JoinSymbol;
                if (columnJoinSymbol != null)
                {
                    symbolPair.Column = columnJoinSymbol.NameToExtent[e.Property.Name];
                    return symbolPair;
                }
                else
                {
                    if (symbolPair.Column.Columns.ContainsKey(e.Property.Name))
                    {
                        result = new SqlBuilder();
                        result.Append(symbolPair.Source);
                        result.Append(".");
                        var columnSymbol = symbolPair.Column.Columns[e.Property.Name];
                        optionalColumnUsageManager.MarkAsUsed(columnSymbol);
                        result.Append(columnSymbol);
                        return result;
                    }
                }
            }
            result = new SqlBuilder();
            result.Append(instanceSql);
            result.Append(".");

            var symbol = instanceSql as Symbol;
            Symbol colSymbol;
            if (symbol != null && symbol.OutputColumns.TryGetValue(e.Property.Name, out colSymbol))
            {
                optionalColumnUsageManager.MarkAsUsed(colSymbol);
                if (symbol.OutputColumnsRenamed)
                {
                    result.Append(colSymbol);
                }
                else
                {
                    result.Append(QuoteIdentifier(e.Property.Name));
                }
            }
            else
            {
                result.Append(QuoteIdentifier(e.Property.Name));
            }
            return result;
        }

        public override ISqlFragment Visit(ScanPredicate e)
        {
            Check.NotNull(e, "e");

            var target = e.Target;

            var targetTSql = GetTargetTSql(target);

            if (_targets != null)
            {
                _targets.Add(targetTSql);
            }

            if (IsParentAJoin)
            {
                var result = new SqlBuilder();
                result.Append(targetTSql);
                return result;
            }
            else
            {
                var result = new SqlSelectStatement();
                result.From.Append(targetTSql);
                return result;
            }
        }

        public override ISqlFragment Visit(VariableReferencePredicate e)
        {
            Check.NotNull(e, "e");

            if (isVarRefSingle)
            {
                throw new NotSupportedException();
            }
            isVarRefSingle = true; 

            var result = symbolTable.Lookup(e.VariableName);
            optionalColumnUsageManager.MarkAsUsed(result);
            if (!CurrentSelectStatement.FromExtents.Contains(result))
            {
                CurrentSelectStatement.OuterExtents[result] = true;
            }
            return result;
        }

        #endregion
    }
}
