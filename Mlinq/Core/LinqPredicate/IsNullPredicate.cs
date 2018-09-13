using Mlinq.Common;
using Mlinq.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mlinq.Core.LinqPredicate
{
     public class IsNullPredicate : UnaryPredicate
    {
         internal IsNullPredicate()
        {
        }

         internal IsNullPredicate(TypeUsage booleanResultType, Predicate arg)
            : base(PredicateType.IsNull, booleanResultType, arg)
        {
        }

         public override void Accept(PredicateVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }

         public override TResultType Accept<TResultType>(PredicateVisitor<TResultType> visitor)
        {
            Check.NotNull(visitor, "visitor");

            return visitor.Visit(this);
        }
    }
}
