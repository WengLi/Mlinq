using Mlinq.Core.LinqPredicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Objects.LinqMethods
{
    internal sealed class SelectTranslator : OneLambdaTranslator
    {
        internal SelectTranslator()
            : base(SequenceMethod.Select)
        {
        }

        internal override Predicate Translate(PredicateConverter parent, MethodCallExpression call)
        {
            Predicate source;
            PredicateBinding sourceBinding;
            Predicate lambda;
            var result = Translate(parent, call, out source, out sourceBinding, out lambda);
            return result;
        }

        protected override Predicate TranslateOneLambda(PredicateConverter parent, PredicateBinding sourceBinding, Predicate lambda)
        {
            return parent.Project(sourceBinding, lambda);
        }
    }
}
