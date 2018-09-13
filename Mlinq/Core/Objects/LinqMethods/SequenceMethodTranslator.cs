using Mlinq.Core.LinqPredicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Objects.LinqMethods
{
    internal abstract class SequenceMethodTranslator
    {
        private readonly IEnumerable<SequenceMethod> _methods;

        protected SequenceMethodTranslator(params SequenceMethod[] methods)
        {
            _methods = methods;
        }

        internal IEnumerable<SequenceMethod> Methods
        {
            get { return _methods; }
        }

        internal virtual Predicate Translate(PredicateConverter parent, MethodCallExpression call, SequenceMethod sequenceMethod)
        {
            return Translate(parent, call);
        }

        internal abstract Predicate Translate(PredicateConverter parent, MethodCallExpression call);

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
