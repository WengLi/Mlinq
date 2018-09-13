using Mlinq.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.LinqPredicate
{
    public abstract class UnaryPredicate : Predicate
    {
        private readonly Predicate _argument;

        internal UnaryPredicate()
        {
        }

        internal UnaryPredicate(PredicateType predicateType, TypeUsage resultType, Predicate argument)
            : base(predicateType, resultType)
        {
            _argument = argument;
        }

        public virtual Predicate Argument
        {
            get { return _argument; }
        }
    }
}
