using Mlinq.Common;
using Mlinq.Core.IServices;
using Mlinq.Core.LinqPredicate;
using Mlinq.Core.LinqPredicate.DbCommandTrees;
using Mlinq.Core.Metadata;
using Mlinq.Core.Objects.Enumerators;
using Mlinq.SqlGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace Mlinq.Core.Objects
{
    public abstract class ObjectQuery : ISqlQuery
    {
        private ObjectQueryProvider _provider;

        internal ObjectQuery()
        {
        }

        internal virtual ObjectQueryProvider ObjectQueryProvider
        {
            get
            {
                if (_provider == null)
                {
                    _provider = new ObjectQueryProvider(this);
                }
                return _provider;
            }
        }

        public Type ElementType
        {
            get { return GetElementType(); }
        }

        public Expression Expression
        {
            get { return GetExpression(); }
        }

        public ISqlQueryProvider Provider
        {
            get { return ObjectQueryProvider; }
        }

        public IEnumerator GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        internal abstract Type GetElementType();
        internal abstract Expression GetExpression();
        internal abstract IEnumerator GetEnumeratorInternal();
    }

    public class ObjectQuery<T> : ObjectQuery, ISqlQuery<T>
    {
        private Expression _expression;

        internal ObjectQuery(Expression expression)
        {
            _expression = expression;
        }

        public new IEnumerator<T> GetEnumerator()
        {
            return new LazyEnumerator<T>(() => GetResults());
        }

        private ObjectResult<T> GetResults()
        {
            PredicateConverter converter = new PredicateConverter(_expression);
            Predicate queryPredicate = converter.Convert();
            var tree = DbQueryCommandTree.FromValidExpression(queryPredicate, false);
            DbDataReader storeReader = null;
            using (var command = CreateCommand(tree))
            {
                if (command.Connection.State == ConnectionState.Closed) 
                {
                    command.Connection.Open();
                }
                storeReader = command.ExecuteReader(CommandBehavior.CloseConnection);
            }
            Func<Shaper, T> element = null;
            if (queryPredicate is ProjectPredicate)
            {
                var projectPredicate = (ProjectPredicate)queryPredicate;
                var rowType = projectPredicate.Projection.ResultType.EdmType as RowType;
                element = GetrowObjectCreator(rowType.NewExpression);
            }
            Shaper<T> shaper = new Shaper<T>(storeReader, element);
            return new ObjectResult<T>(shaper);
        }

        private SqlCommand CreateCommand(DbQueryCommandTree commandTree)
        {
            string ConnectionString = @"Data Source=DESKTOP-QA0P2T2\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa;Password=123";
            if (DbCommandTreeKind.Query == commandTree.CommandTreeKind)
            {
                var command = new SqlCommand();
                List<SqlParameter> parameters;
                CommandType commandType;
                HashSet<string> paramsToForceNonUnicode;
                command.CommandText = SqlGenerator.GenerateSql(commandTree, out parameters, out commandType, out paramsToForceNonUnicode);
                command.CommandType = commandType;
                command.Connection = new SqlConnection(ConnectionString);
                return command;
            }
            throw new Exception();
        }

        private Func<Shaper, T> GetrowObjectCreator(NewExpression newExp)
        {          
            Expression nullSentinelCheck = null;
            Expression result = null;
            List<Expression> expressionList = new List<Expression>();

            for (int n = 0; n < newExp.Arguments.Count; n++)
            {
                bool needsNullableCheck;
                Type type = newExp.Arguments[n].Type;

                var readerMethod = CodeGenEmitter.GetReaderMethod(type, out needsNullableCheck);
                if (needsNullableCheck && nullSentinelCheck == null)
                {
                    nullSentinelCheck = CodeGenEmitter.Emit_Reader_IsDBNull(0);
                }
                Expression element = Expression.Call(CodeGenEmitter.Shaper_Reader, readerMethod, Expression.Constant(n));

                Type nonNullableType = type.GetNonNullableType();
                if (nonNullableType.IsEnum() && nonNullableType != type)
                {
                    element = Expression.Convert(element, nonNullableType);
                }

                if (needsNullableCheck)
                {
                    element = CodeGenEmitter.Emit_Conditional_NotDBNull(element, n, type);
                }

                expressionList.Add(element);
            }
            result = Expression.New(newExp.Constructor, expressionList);
            Expression nullConstant = CodeGenEmitter.Emit_NullConstant(result.Type);
            if (nullSentinelCheck != null)
            {
                result = Expression.Condition(nullSentinelCheck, nullConstant, result);
            }

            Expression<Func<Shaper, T>> lambda = CodeGenEmitter.BuildShaperLambda<T>(result);
            return lambda.Compile();
        }

        internal override Type GetElementType()
        {
            return typeof(T);
        }

        internal override Expression GetExpression()
        {
            if (_expression == null)
                return Expression.Constant(this);
            else
                return _expression;
        }

        internal override IEnumerator GetEnumeratorInternal()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }
    }
}
