using Mlinq.Common;
using Mlinq.Core.IServices;
using System;
using System.Linq.Expressions;

namespace Mlinq.Core.Objects
{
    internal class ObjectQueryProvider : ISqlQueryProvider
    {
        private readonly ObjectQuery _query;

        internal ObjectQueryProvider(ObjectQuery query)
        {
            _query = query;
        }


        ISqlQuery<TElement> ISqlQueryProvider.CreateQuery<TElement>(Expression expression)
        {
            Check.NotNull(expression, "expression");

            if (!typeof(ISqlQuery<TElement>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentException("expression");
            }

            return CreateQuery<TElement>(expression);
        }

        internal virtual ObjectQuery<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new ObjectQuery<TElement>(expression);
        }

        TResult ISqlQueryProvider.Execute<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}
