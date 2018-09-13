using Mlinq.Common;
using Mlinq.Internal;
using System;

namespace Mlinq
{
    public class DbContext : IDisposable
    {
        public DbContext(string nameOrConnectionString)
        {
            Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");
            new DbSetDiscoveryService(this).InitializeSets();
            
        }

        public virtual DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return new DbSet<TEntity>(this);
        }

        public void Dispose()
        {
        }
    }
}
