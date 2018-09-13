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
    public class ConstantPredicate : Predicate
    {
        private readonly object _value;

        internal ConstantPredicate()
        {
        }

        internal ConstantPredicate(TypeUsage resultType, object value)
            : base(PredicateType.Constant, resultType)
        {
            _value = value;
        }

        internal object GetValue()
        {
            return _value;
        }

        public virtual object Value
        {
            get
            {
                return _value;
            }
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
