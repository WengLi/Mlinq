using Mlinq.Common;
using Mlinq.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.LinqPredicate
{
    public sealed class ComparisonPredicate : BinaryPredicate
    {
        internal ComparisonPredicate(PredicateType predicateType, TypeUsage booleanResultType, Predicate left, Predicate right)
            : base(predicateType, booleanResultType, left, right)
        {
        }

        [DebuggerNonUserCode]
        public override void Accept(PredicateVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }

        [DebuggerNonUserCode]
        public override TResultType Accept<TResultType>(PredicateVisitor<TResultType> visitor)
        {
            Check.NotNull(visitor, "visitor");

            return visitor.Visit(this);
        }
    }
}
