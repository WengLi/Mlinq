using Mlinq.Common;
using Mlinq.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.LinqPredicate
{
    public sealed class NewInstancePredicate : Predicate
    {
        private readonly PredicateList _elements;

        internal NewInstancePredicate(TypeUsage type, PredicateList args)
            : base(PredicateType.NewInstance, type)
        {
            _elements = args;
        }

        public IList<Predicate> Arguments
        {
            get { return _elements; }
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
