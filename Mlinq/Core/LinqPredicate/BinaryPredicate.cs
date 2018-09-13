using Mlinq.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.LinqPredicate
{
    public abstract class BinaryPredicate : Predicate
    {
        private readonly Predicate _left;
        private readonly Predicate _right;

        internal BinaryPredicate()
        {
        }

        internal BinaryPredicate(PredicateType predicateType, TypeUsage type, Predicate left, Predicate right)
            : base(predicateType, type)
        {
            _left = left;
            _right = right;
        }

        public virtual Predicate Left
        {
            get { return _left; }
        }

        public virtual Predicate Right
        {
            get { return _right; }
        }
    }
}
