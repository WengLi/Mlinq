using Mlinq.Common;
using Mlinq.Core.Metadata;
using System.Diagnostics;

namespace Mlinq.Core.LinqPredicate
{
    public sealed class FilterPredicate : Predicate    
    {
        private readonly PredicateBinding _input;
        private readonly Predicate _predicate;

        internal FilterPredicate(TypeUsage resultType, PredicateBinding input, Predicate predicate)
            : base(PredicateType.Filter, resultType)
        {
            _input = input;
            _predicate = predicate;
        }

        public PredicateBinding Input
        {
            get { return _input; }
        }

        public Predicate Predicate
        {
            get { return _predicate; }
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