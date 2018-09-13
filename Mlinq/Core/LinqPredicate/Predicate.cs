using Mlinq.Core.Metadata;

namespace Mlinq.Core.LinqPredicate
{
    public abstract class Predicate
    {
        private readonly TypeUsage _type;

        private readonly PredicateType _predicateType;

        public Predicate()
        { }

        internal Predicate(PredicateType predicateType, TypeUsage type)
        {
            _predicateType = predicateType;
            _type = type;
        }

        public virtual TypeUsage ResultType
        {
            get { return _type; }
        }

        public virtual PredicateType PredicateType
        {
            get { return _predicateType; }
        }

        public abstract void Accept(PredicateVisitor visitor);

        public abstract TResultType Accept<TResultType>(PredicateVisitor<TResultType> visitor);
    }
}
