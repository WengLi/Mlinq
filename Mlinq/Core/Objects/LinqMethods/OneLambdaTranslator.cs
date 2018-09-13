using Mlinq.Common;
using Mlinq.Core.LinqPredicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Objects.LinqMethods
{
    internal abstract class OneLambdaTranslator : SequenceMethodTranslator
    {
        internal OneLambdaTranslator(params SequenceMethod[] methods)
            : base(methods)
        {
        }

        internal override Predicate Translate(PredicateConverter parent, MethodCallExpression call)
        {
            Predicate source;
            PredicateBinding sourceBinding;
            Predicate lambda;
            return Translate(parent, call, out source, out sourceBinding, out lambda);
        }

        protected Predicate Translate(PredicateConverter parent, MethodCallExpression call, out Predicate source, out PredicateBinding sourceBinding, out Predicate lambda)
        {
            source = parent.TranslateExpression(call.Arguments[0]);

            var lambdaExpression = parent.GetLambdaExpression(call, 1);
            lambda = parent.TranslateLambda(lambdaExpression, source, out sourceBinding);
            return TranslateOneLambda(parent, sourceBinding, lambda);
        }

        protected abstract Predicate TranslateOneLambda(PredicateConverter parent, PredicateBinding sourceBinding, Predicate lambda);
    }
}
