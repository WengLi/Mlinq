using Mlinq.Common;
using Mlinq.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mlinq.Core.LinqPredicate
{
    public sealed class NotPredicate : UnaryPredicate
    {
        internal NotPredicate(TypeUsage booleanResultType, Predicate argument)
            : base(PredicateType.Not, booleanResultType, argument)
        {
        }

        public override void Accept(PredicateVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }

        public override TResultType Accept<TResultType>(PredicateVisitor<TResultType> visitor)
        {
            Check.NotNull(visitor, "visitor");

            return visitor.Visit(this);
        }
    }
}
