using System.Linq.Expressions;

namespace Mlinq.Core.IServices
{
    public interface ISqlQueryProvider
    {
        ISqlQuery<TElement> CreateQuery<TElement>(Expression expression);
        TResult Execute<TResult>(Expression expression);
    }
}
