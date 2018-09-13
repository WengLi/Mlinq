using Mlinq.Core.LinqPredicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Mlinq.Core.Objects.Translators
{
    internal abstract class Translator
    {
        private readonly ExpressionType[] _nodeTypes;

        protected Translator(params ExpressionType[] nodeTypes)
        {
            _nodeTypes = nodeTypes;
        }

        internal IEnumerable<ExpressionType> NodeTypes
        {
            get { return _nodeTypes; }
        }

        internal abstract Predicate Translate(PredicateConverter parent, Expression linq);

        public override string ToString()
        {
            return GetType().Name;
        }
    }

    internal enum EqualsPattern
    {
        Store, // defer to store
        PositiveNullEqualityNonComposable,
        // simulate C# semantics in store, return "null" if left or right is null, but not both. Suitable for joins.
        PositiveNullEqualityComposable, // simulate C# semantics in store, always return true or false
    }
}
