using Mlinq.Core.LinqPredicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Objects.LinqMethods
{
    internal sealed class WhereTranslator : OneLambdaTranslator
    {
        internal WhereTranslator()
            : base(SequenceMethod.Where)
        {
        }

        protected override Predicate TranslateOneLambda(PredicateConverter parent, PredicateBinding sourceBinding, Predicate lambda)
        {
            return parent.Filter(sourceBinding, lambda);
        }
    }
}
