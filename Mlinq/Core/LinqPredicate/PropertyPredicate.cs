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
    public class PropertyPredicate : Predicate
    {
        private readonly EdmProperty _property;
        private readonly Predicate _instance;

        internal PropertyPredicate()
        {
        }

        internal PropertyPredicate(TypeUsage resultType, EdmProperty property, Predicate instance)
            : base(PredicateType.Property, resultType)
        {
            _property = property;
            _instance = instance;
        }

        public virtual EdmProperty Property
        {
            get { return _property; }
        }


        public virtual Predicate Instance
        {
            get { return _instance; }
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

        public KeyValuePair<string, Predicate> ToKeyValuePair()
        {
            return new KeyValuePair<string, Predicate>(Property.Name, this);
        }

        public static implicit operator KeyValuePair<string, Predicate>(PropertyPredicate value)
        {
            Check.NotNull(value, "value");

            return value.ToKeyValuePair();
        }
    }
}
