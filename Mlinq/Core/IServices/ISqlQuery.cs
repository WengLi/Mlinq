using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mlinq.Core.IServices
{
    public interface ISqlQuery : IEnumerable
    {
        Type ElementType { get; }
        Expression Expression { get; }
        ISqlQueryProvider Provider { get; }
    }

    public interface ISqlQuery<TElement> : IEnumerable<TElement>, ISqlQuery
    {
    }
}
