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
    public sealed class ProjectPredicate : Predicate
    {
        private readonly PredicateBinding _input;
        private readonly Predicate _projection;

        internal ProjectPredicate(TypeUsage resultType, PredicateBinding input, Predicate projection)
            : base(PredicateType.Project, resultType)
        {
            _input = input;
            _projection = projection;
        }

        public PredicateBinding Input
        {
            get { return _input; }
        }

        public Predicate Projection
        {
            get { return _projection; }
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
