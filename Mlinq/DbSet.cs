using Mlinq.Common;
using Mlinq.Core.IServices;
using Mlinq.Core.Objects;
using Mlinq.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mlinq
{
    public class DbSet<TEntity> : IDbSet<TEntity> where TEntity : class
    {    
        private readonly InternalSet<TEntity> _internalSet;

        internal DbSet(DbContext owner)
        {
            Check.NotNull(owner, "context");
            _internalSet = new InternalSet<TEntity>(owner);
        }
        
        #region impl
        public IEnumerator<TEntity> GetEnumerator()
        {
            return ((IEnumerable<TEntity>)_internalSet.ObjectQuery).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Type ElementType
        {
            get { return typeof(TEntity); }
        }

        public Expression Expression
        {
            get
            {
                return ((ISqlQuery)_internalSet.ObjectQuery).Expression;
            }
        }

        public ISqlQueryProvider Provider
        {
            get
            {
                return _internalSet.ObjectQuery.Provider;
            }
        }
        #endregion
    }
}
