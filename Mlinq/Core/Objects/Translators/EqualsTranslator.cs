using Mlinq.Core.LinqPredicate;
using Mlinq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Objects.Translators
{
    internal sealed class EqualsTranslator : TypedTranslator<BinaryExpression>
    {
        internal EqualsTranslator()
            : base(ExpressionType.Equal)
        {
        }

        protected override Predicate TypedTranslate(PredicateConverter parent, BinaryExpression linq)
        {
            var linqLeft = linq.Left;
            var linqRight = linq.Right;

            var leftIsNull = linqLeft.IsNullConstant();
            var rightIsNull = linqRight.IsNullConstant();

            if (leftIsNull && rightIsNull)
            {
                return PredicateBuilder.True;
            }

            if (leftIsNull)
            {
                return CreateIsNullExpression(parent, linqRight);
            }
            if (rightIsNull)
            {
                return CreateIsNullExpression(parent, linqLeft);
            }

            var cqtLeft = parent.TranslateExpression(linqLeft);
            var cqtRight = parent.TranslateExpression(linqRight);
            var pattern = EqualsPattern.Store;

            return parent.CreateEqualsExpression(cqtLeft, cqtRight, pattern, linqLeft.Type, linqRight.Type);
        }

        private static Predicate CreateIsNullExpression(PredicateConverter parent, Expression input)
        {
            input = input.RemoveConvert();

            var inputCqt = parent.TranslateExpression(input);

            return PredicateConverter.CreateIsNullExpression(inputCqt, input.Type);
        }
    }
}
