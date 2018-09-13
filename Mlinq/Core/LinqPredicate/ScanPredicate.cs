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
    public class ScanPredicate : Predicate
    {
        private readonly EntitySet _targetSet;

        internal ScanPredicate()
        {
        }

        internal ScanPredicate(TypeUsage collectionOfEntityType, EntitySet entitySet)
            : base(PredicateType.Scan, collectionOfEntityType)
        {
            _targetSet = entitySet;
        }

        public virtual EntitySet Target
        {
            get { return _targetSet; }
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
