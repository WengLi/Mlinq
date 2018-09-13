using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.LinqPredicate
{
    public abstract class PredicateVisitor
    {
        internal void Visit(Predicate e)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class PredicateVisitor<TResultType>
    {
        public abstract TResultType Visit(Predicate e);
        public abstract TResultType Visit(ComparisonPredicate e);
        public abstract TResultType Visit(ConstantPredicate e);
        public abstract TResultType Visit(FilterPredicate e);
        public abstract TResultType Visit(NewInstancePredicate e);
        public abstract TResultType Visit(ProjectPredicate e);
        public abstract TResultType Visit(PropertyPredicate e);
        public abstract TResultType Visit(ScanPredicate e);
        public abstract TResultType Visit(VariableReferencePredicate e);
    }
}
