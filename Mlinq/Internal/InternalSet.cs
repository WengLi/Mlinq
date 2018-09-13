using Mlinq.Core.Metadata;
using Mlinq.Core.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Internal
{
    internal class InternalSet<TEntity> where TEntity : class
    {
        private ObjectQuery<TEntity> _objectQuery;
        private readonly DbContext _owner;
        private EntitySet _entitySet;
        private string _entitySetName;
        private string _quotedEntitySetName;
        private Type _baseType;

        public ObjectQuery<TEntity> ObjectQuery
        {
            get
            {
                return _objectQuery;
            }
        }

        public string EntitySetName
        {
            get
            {
                return _entitySetName;
            }
        }

        public string QuotedEntitySetName
        {
            get
            {
                return _quotedEntitySetName;
            }
        }

        public EntitySet EntitySet
        {
            get
            {
                return _entitySet;
            }
        }

        public Type EntitySetBaseType
        {
            get
            {
                return _baseType;
            }
        }

        public InternalSet(DbContext owner)
        {
            _owner = owner;
            Initialize();
        }

        public virtual void Initialize()
        {
            if (_entitySet == null)
            {
                var pair = GetEntitySetAndBaseTypeForType(typeof(TEntity));

                if (_entitySet == null)
                {
                    InitializeUnderlyingTypes(pair);
                }
            }
        }

        private EntitySetTypePair GetEntitySetAndBaseTypeForType(Type type)
        {
            return EntitySetCache.EntitySetMappingCache[type];
        }

        private void InitializeUnderlyingTypes(EntitySetTypePair pair)
        {
            _entitySet = pair.EntitySet;
            _baseType = pair.BaseType;

            _entitySetName = string.Format(
                CultureInfo.InvariantCulture, "{0}.{1}", _entitySet.EntityContainer.Name, _entitySet.Name);
            _quotedEntitySetName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}.{1}",
                QuoteIdentifier(_entitySet.EntityContainer.Name),
                QuoteIdentifier(_entitySet.Name));

            InitializeQuery(CreateObjectQuery());
        }

        protected void InitializeQuery(ObjectQuery<TEntity> objectQuery)
        {
            _objectQuery = objectQuery;
        }

        private ObjectQuery<TEntity> CreateObjectQuery()
        {
            var query = new ObjectQuery<TEntity>(null);
            return query;
        }

        private string QuoteIdentifier(string identifier)
        {
            return "[" + identifier.Replace("]", "]]") + "]";
        }
    }
}
