using Mlinq.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mlinq.Internal
{
    internal class DbSetDiscoveryService
    {
        public static readonly MethodInfo SetMethod = typeof(DbContext).GetDeclaredMethod("Set");
        private static readonly ConcurrentDictionary<Type, DbContextTypesInitializersPair> _objectSetInitializers = new ConcurrentDictionary<Type, DbContextTypesInitializersPair>();
        private readonly DbContext _context;

        public DbSetDiscoveryService(DbContext context)
        {
            _context = context;
        }

        public void InitializeSets()
        {
            GetSets();
            _objectSetInitializers[_context.GetType()].SetsInitializer(_context);           
        }

        private Dictionary<Type, List<string>> GetSets()
        {
            DbContextTypesInitializersPair setsInfo;
            if (!_objectSetInitializers.TryGetValue(_context.GetType(), out setsInfo))
            {
                var dbContextParam = Expression.Parameter(typeof(DbContext), "dbContext");
                var initDelegates = new List<Action<DbContext>>();

                var entityTypes = new Dictionary<Type, List<string>>();

                foreach (var propertyInfo in _context.GetType().GetInstanceProperties().Where(p => p.GetIndexParameters().Length == 0 && p.DeclaringType != typeof(DbContext)))
                {
                    var entityType = GetSetType(propertyInfo.PropertyType);
                    if (entityType != null)
                    {
                        if (!entityType.IsValidStructuralType())
                        {
                            throw new InvalidOperationException(entityType.Name);
                        }

                        List<string> properties;
                        if (!entityTypes.TryGetValue(entityType, out properties))
                        {
                            properties = new List<string>();
                            entityTypes[entityType] = properties;
                        }
                        properties.Add(propertyInfo.Name);

                        var setter = propertyInfo.Setter();
                        if (setter != null && setter.IsPublic)
                        {
                            var setMethod = SetMethod.MakeGenericMethod(entityType);

                            var newExpression = Expression.Call(dbContextParam, setMethod);
                            var setExpression = Expression.Call(Expression.Convert(dbContextParam, _context.GetType()), setter, newExpression);
                            initDelegates.Add(Expression.Lambda<Action<DbContext>>(setExpression, dbContextParam).Compile());
                        }
                    }
                }

                Action<DbContext> initializer = dbContext =>
                {
                    foreach (var initer in initDelegates)
                    {
                        initer(dbContext);
                    }
                };

                setsInfo = new DbContextTypesInitializersPair(entityTypes, initializer);
                _objectSetInitializers.TryAdd(_context.GetType(), setsInfo);
                EntitySetCache.Initialize(entityTypes);
            }
            return setsInfo.EntityTypeToPropertyNameMap;
        }

        private static Type GetSetType(Type declaredType)
        {
            if (!declaredType.IsArray)
            {
                var entityType = GetSetElementType(declaredType);
                if (entityType != null)
                {
                    var setOfT = typeof(DbSet<>).MakeGenericType(entityType);
                    if (declaredType.IsAssignableFrom(setOfT))
                    {
                        return entityType;
                    }
                }
            }

            return null;
        }

        private static Type GetSetElementType(Type setType)
        {
            try
            {
                var setInterface = (setType.IsGenericType() && typeof(IDbSet<>).IsAssignableFrom(setType.GetGenericTypeDefinition())) ? setType : setType.GetInterface(typeof(IDbSet<>).FullName);

                if (setInterface != null && !setInterface.ContainsGenericParameters())
                {
                    return setInterface.GetGenericArguments()[0];
                }
            }
            catch (AmbiguousMatchException)
            {
            }
            return null;
        }

        #region nest class
        private class DbContextTypesInitializersPair : Tuple<Dictionary<Type, List<string>>, Action<DbContext>>
        {
            public DbContextTypesInitializersPair(Dictionary<Type, List<string>> entityTypeToPropertyNameMap, Action<DbContext> setsInitializer)
                : base(entityTypeToPropertyNameMap, setsInitializer)
            {
            }

            public Dictionary<Type, List<string>> EntityTypeToPropertyNameMap
            {
                get { return Item1; }
            }

            public Action<DbContext> SetsInitializer
            {
                get { return Item2; }
            }
        }
        #endregion
    }
}
