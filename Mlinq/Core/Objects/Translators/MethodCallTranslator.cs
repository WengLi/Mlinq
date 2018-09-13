using Mlinq.Common;
using Mlinq.Core.LinqPredicate;
using Mlinq.Core.Objects.LinqMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Objects.Translators
{
    internal sealed partial class MethodCallTranslator : TypedTranslator<MethodCallExpression>
    {
        private static readonly Dictionary<SequenceMethod, SequenceMethodTranslator> _sequenceTranslators = InitializeSequenceMethodTranslators();
        internal MethodCallTranslator()
            : base(ExpressionType.Call)
        {
        }
        protected override Predicate TypedTranslate(PredicateConverter parent, MethodCallExpression linq)
        {
            SequenceMethod sequenceMethod;
            SequenceMethodTranslator sequenceTranslator;
            if (ReflectionUtil.TryIdentifySequenceMethod(linq.Method, out sequenceMethod) && _sequenceTranslators.TryGetValue(sequenceMethod, out sequenceTranslator))
            {
                return sequenceTranslator.Translate(parent, linq, sequenceMethod);
            }
            else
            {
                throw new Exception();
            }
        }

        private static Dictionary<SequenceMethod, SequenceMethodTranslator> InitializeSequenceMethodTranslators()
        {
            var sequenceTranslators = new Dictionary<SequenceMethod, SequenceMethodTranslator>();
            foreach (var translator in GetSequenceMethodTranslators())
            {
                foreach (var method in translator.Methods)
                {
                    sequenceTranslators.Add(method, translator);
                }
            }

            return sequenceTranslators;
        }

        private static IEnumerable<SequenceMethodTranslator> GetSequenceMethodTranslators()
        {
            yield return new WhereTranslator();
            yield return new SelectTranslator();
        }
    }
}
