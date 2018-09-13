using Mlinq.Common;
using Mlinq.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.LinqPredicate
{
    public class VariableReferencePredicate : Predicate
    {
        private readonly string _name;

        internal VariableReferencePredicate()
        {
        }

        internal VariableReferencePredicate(TypeUsage type, string name)
            : base(PredicateType.VariableReference, type)
        {
            _name = name;
        }

        public virtual string VariableName
        {
            get { return _name; }
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
