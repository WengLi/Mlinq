using Mlinq.Core.LinqPredicate;
using System;
using System.Linq.Expressions;

namespace Mlinq.Core.Objects.Translators
{
    internal sealed class NotSupportedTranslator : Translator
    {
        internal NotSupportedTranslator(params ExpressionType[] nodeTypes)
            : base(nodeTypes)
        {
        }

        internal override Predicate Translate(PredicateConverter parent, Expression linq)
        {
            throw new NotSupportedException();
        }
    }
}
