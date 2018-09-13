using Mlinq.Core.LinqPredicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Mlinq.Core.Objects
{
    internal sealed class Binding
    {
        internal Binding(Expression linqExpression, Predicate predicate)
        {
            LinqExpression = linqExpression;
            Predicate = predicate;
        }

        internal readonly Expression LinqExpression;
        internal readonly Predicate Predicate;
    }

    internal sealed class BindingContext
    {
        private readonly Stack<Binding> _scopes;

        internal BindingContext()
        {
            _scopes = new Stack<Binding>();
        }

        internal void PushBindingScope(Binding binding)
        {
            _scopes.Push(binding);
        }

        internal void PopBindingScope()
        {
            _scopes.Pop();
        }

        internal bool TryGetBoundExpression(Expression linqExpression, out Predicate predicate)
        {
            predicate = _scopes
                .Where(binding => binding.LinqExpression == linqExpression)
                .Select(binding => binding.Predicate)
                .FirstOrDefault();
            return predicate != null;
        }
    }
}
