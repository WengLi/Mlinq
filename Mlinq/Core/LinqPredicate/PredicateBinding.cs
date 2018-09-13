using Mlinq.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.LinqPredicate
{
    public sealed class PredicateBinding
    {
        private readonly Predicate _predicate;
        private readonly VariableReferencePredicate _varRef;

        internal PredicateBinding(Predicate input, VariableReferencePredicate varRef)
        {
            _predicate = input;
            _varRef = varRef;
        }

        public Predicate Predicate
        {
            get { return _predicate; }
        }

        public string VariableName
        {
            get { return _varRef.VariableName; }
        }

        public TypeUsage VariableType
        {
            get { return _varRef.ResultType; }
        }

        public VariableReferencePredicate Variable
        {
            get { return _varRef; }
        }
    }
}
