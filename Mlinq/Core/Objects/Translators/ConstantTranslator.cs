using Mlinq.Core.LinqPredicate;
using Mlinq.Core.Metadata;
using Mlinq.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Objects.Translators
{
    internal sealed class ConstantTranslator
            : TypedTranslator<ConstantExpression>
    {
        internal ConstantTranslator()
            : base(ExpressionType.Constant)
        {
        }

        protected override Predicate TypedTranslate(PredicateConverter parent, ConstantExpression linq)
        {
            var queryOfT = (linq.Value as ObjectQuery);
            if (queryOfT != null)
            {
                return parent.TranslateInlineQueryOfT(queryOfT);
            }

            var values = linq.Value as IEnumerable;
            if (values != null)
            {
                var elementType = linq.Type.GetNonEnumElementType();
                if ((elementType != null) && (elementType != linq.Type))
                {
                    var expressions = new List<Expression>();
                    foreach (var o in values)
                    {
                        expressions.Add(Expression.Constant(o, elementType));
                    }
                    return parent.TranslateExpression(Expression.NewArrayInit(elementType, expressions));
                }
            }

            return PredicateBuilder.Constant(linq.Value);
        }
    }
}
