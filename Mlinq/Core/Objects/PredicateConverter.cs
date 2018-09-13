using Mlinq.Common;
using Mlinq.Core.LinqPredicate;
using Mlinq.Core.Metadata;
using Mlinq.Core.Objects.Translators;
using Mlinq.Internal;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Mlinq.Core.Objects
{
    internal sealed class PredicateConverter
    {
        private readonly Expression _expression;
        private readonly BindingContext _bindingContext;
        private HashSet<ObjectQuery> _inlineEntitySqlQueries;
        private int _ignoreInclude;
        private readonly AliasGenerator _aliasGenerator = new AliasGenerator("LQ", 0);
        private static readonly Dictionary<ExpressionType, Translator> _translators = InitializeTranslators();


        internal PredicateConverter(Expression expression)
        {
            _expression = expression;
            _bindingContext = new BindingContext();
            _ignoreInclude = 0;
        }

        private static Dictionary<ExpressionType, Translator> InitializeTranslators()
        {
            var translators = new Dictionary<ExpressionType, Translator>();
            foreach (var translator in GetTranslators())
            {
                foreach (var nodeType in translator.NodeTypes)
                {
                    translators.Add(nodeType, translator);
                }
            }

            return translators;
        }

        private static IEnumerable<Translator> GetTranslators()
        {
            yield return new ConstantTranslator();
            yield return new MemberAccessTranslator();
            yield return new NewTranslator();
            yield return new MethodCallTranslator();
            yield return new EqualsTranslator();
            yield return new NotSupportedTranslator(
                ExpressionType.LeftShift,
                ExpressionType.RightShift,
                ExpressionType.ArrayLength,
                ExpressionType.ArrayIndex,
                ExpressionType.Invoke,
                ExpressionType.Lambda,
                ExpressionType.NewArrayBounds);
        }

        internal Predicate Convert()
        {
            var result = TranslateExpression(_expression);
            return result;
        }

        internal Predicate TranslateExpression(Expression linq)
        {
            Predicate result;
            if (!_bindingContext.TryGetBoundExpression(linq, out result))
            {
                Translator translator;
                if (_translators.TryGetValue(linq.NodeType, out translator))
                {
                    result = translator.Translate(this, linq);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            return result;
        }

        internal Predicate TranslateInlineQueryOfT(ObjectQuery inlineQuery)
        {
            if (null == _inlineEntitySqlQueries)
            {
                _inlineEntitySqlQueries = new HashSet<ObjectQuery>();
            }
            var isNewInlineQuery = _inlineEntitySqlQueries.Add(inlineQuery);

            var pair = EntitySetCache.EntitySetMappingCache[inlineQuery.GetType().GetGenericArguments()[0]];
            Predicate converted = pair.EntitySet.Scan();
            if (converted is ScanPredicate)
            {
                var source = converted.BindAs("extent");
                converted = source.Project(source.Variable);
            }
            return converted;
        }

        internal LambdaExpression GetLambdaExpression(MethodCallExpression callExpression, int argumentOrdinal)
        {
            var argument = callExpression.Arguments[argumentOrdinal];
            return (LambdaExpression)GetLambdaExpression(argument);
        }

        private Expression GetLambdaExpression(Expression argument)
        {
            if (ExpressionType.Lambda == argument.NodeType)
            {
                return argument;
            }
            else if (ExpressionType.Quote == argument.NodeType)
            {
                return GetLambdaExpression(((UnaryExpression)argument).Operand);
            }
            else if (ExpressionType.Call  == argument.NodeType)
            {
                if (typeof(Expression).IsAssignableFrom(argument.Type))
                {
                    var expressionMethod = Expression.Lambda<Func<Expression>>(argument).Compile();

                    return GetLambdaExpression(expressionMethod.Invoke());
                }
            }
            else if (ExpressionType.Invoke == argument.NodeType)
            {
                if (typeof(Expression).IsAssignableFrom(argument.Type))
                {
                    var expressionMethod = Expression.Lambda<Func<Expression>>(argument).Compile();

                    return GetLambdaExpression(expressionMethod.Invoke());
                }
            }

            throw new InvalidOperationException();
        }

        internal Predicate TranslateLambda(LambdaExpression lambda, Predicate input, out PredicateBinding binding)
        {
            input = NormalizeSetSource(input);
            binding = input.BindAs(_aliasGenerator.Next());
            return TranslateLambda(lambda, binding.Variable);
        }

        private Predicate NormalizeSetSource(Predicate input)
        {
            if (input.PredicateType == PredicateType.Project)
            {
                var project = (ProjectPredicate)input;
                if (project.Projection == project.Input.Variable)
                {
                    input = project.Input.Predicate;
                }
            }
            return input;
        }

        private Predicate TranslateLambda(LambdaExpression lambda, Predicate input)
        {
            var scopeBinding = new Binding(lambda.Parameters[0], input);
            _bindingContext.PushBindingScope(scopeBinding);
            _ignoreInclude++;
            var result = TranslateExpression(lambda.Body);
            _ignoreInclude--;
            _bindingContext.PopBindingScope();
            return result;
        }

        internal Predicate Project(PredicateBinding input, Predicate projection)
        {
            return input.Project(projection);
        }

        internal Predicate Filter(PredicateBinding input, Predicate predicate)
        {
            return input.Filter(predicate);
        }

        internal Predicate CreateEqualsExpression(Predicate left, Predicate right, EqualsPattern pattern, Type leftClrType, Type rightClrType)
        {
            return RecursivelyRewriteEqualsExpression(left, right, pattern);
        }

        private Predicate RecursivelyRewriteEqualsExpression(Predicate left, Predicate right, EqualsPattern pattern)
        {
            var leftType = left.ResultType.EdmType as RowType;
            var rightType = right.ResultType.EdmType as RowType;

            if (null != leftType || null != rightType)
            {
                if (null != leftType && null != rightType)
                {
                    Predicate shreddedEquals = null;
                    foreach (var property in leftType.Properties)
                    {
                        var leftElement = left.Property(property);
                        var rightElement = right.Property(property);
                        var elementsEquals = RecursivelyRewriteEqualsExpression(leftElement, rightElement, pattern);

                        if (null == shreddedEquals)
                        {
                            shreddedEquals = elementsEquals;
                        }
                        else
                        {
                            shreddedEquals = shreddedEquals.And(elementsEquals);
                        }
                    }
                    return shreddedEquals;
                }
                else
                {
                    return PredicateBuilder.False;
                }
            }
            else
            {
                return ImplementEquality(left, right, pattern);
            }
        }

        private Predicate ImplementEquality(Predicate left, Predicate right, EqualsPattern pattern)
        {
            switch (left.PredicateType)
            {
                case PredicateType.Constant:
                    switch (right.PredicateType)
                    {
                        case PredicateType.Constant: // constant EQ constant
                            return left.Equal(right);
                        case PredicateType.Null: // null EQ constant --> false
                            return PredicateBuilder.False;
                        default:
                            return ImplementEqualityConstantAndUnknown((ConstantPredicate)left, right, pattern);
                    }
                case PredicateType.Null:
                    switch (right.PredicateType)
                    {
                        case PredicateType.Constant:
                            return PredicateBuilder.False;
                        case PredicateType.Null:
                            return PredicateBuilder.True;
                        default:
                            return right.IsNull();
                    }
                default:
                    switch (right.PredicateType)
                    {
                        case PredicateType.Constant:
                            return ImplementEqualityConstantAndUnknown((ConstantPredicate)right, left, pattern);
                        case PredicateType.Null:
                            return left.IsNull();
                        default:
                            return ImplementEqualityUnknownArguments(left, right, pattern);
                    }
            }
        }

        private Predicate ImplementEqualityConstantAndUnknown(ConstantPredicate constant, Predicate unknown, EqualsPattern pattern)
        {
            switch (pattern)
            {
                case EqualsPattern.Store:
                case EqualsPattern.PositiveNullEqualityNonComposable: // for Joins                    
                    return constant.Equal(unknown);
                case EqualsPattern.PositiveNullEqualityComposable:
                    return constant.Equal(unknown).And(unknown.IsNull().Not());
                default:
                    return null;
            }
        }

        private Predicate ImplementEqualityUnknownArguments(Predicate left, Predicate right, EqualsPattern pattern)
        {
            switch (pattern)
            {
                case EqualsPattern.Store:
                    return left.Equal(right);
                case EqualsPattern.PositiveNullEqualityNonComposable: // for Joins
                    return left.Equal(right).Or(left.IsNull().And(right.IsNull()));
                case EqualsPattern.PositiveNullEqualityComposable:
                    {
                        var bothNotNull = left.Equal(right);
                        var bothNull = left.IsNull().And(right.IsNull());
                        var anyOneIsNull = left.IsNull().Or(right.IsNull());
                        return (bothNotNull.And(anyOneIsNull.Not())).Or(bothNull);
                    }
                default:
                    return null;
            }
        }

        internal static Predicate CreateIsNullExpression(Predicate operand, Type operandClrType)
        {
            return operand.IsNull();
        }
    }
}
