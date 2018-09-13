using Mlinq.Core.IServices;

namespace Mlinq
{
    public interface IDbSet<TEntity> : ISqlQuery<TEntity> where TEntity : class
    {
    }
}
