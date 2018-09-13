using Mlinq.Common;
using Mlinq.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mlinq.Internal
{
    internal class EntitySetCache
    {
        private static readonly Dictionary<Type, EntitySetTypePair> _entitySetMappingsCache = new Dictionary<Type, EntitySetTypePair>();
        private static object _object = new object();

        internal static IDictionary<Type, EntitySetTypePair> EntitySetMappingCache
        {
            get { return _entitySetMappingsCache; }
        }

        internal static void Initialize(Dictionary<Type, List<string>> sets)
        {
            if (_entitySetMappingsCache.Count > 0)
            {
                return;
            }
            lock (_object)
            {
                foreach (var set in sets)
                {
                    if (set.Value.Count > 1)
                    {
                        throw new InvalidOperationException();
                    }

                    var type = set.Key;

                    if (_entitySetMappingsCache.ContainsKey(type))
                    {
                        continue;
                    }
                    
                    EntityType entityType = new EntityType(type.Name, type.NestingNamespace(), null);
                    var entitySet = new EntitySet(type.Name, "dbo", type.Name, entityType);

                    var properties = GetProperties(type, false, new Type[] { type }).ToList();
                    for (var i = 0; i < properties.Count; ++i)
                    {
                        var propertyInfo = properties[i];
                        EdmProperty property = new EdmProperty();
                        property.PropertyInfo = propertyInfo;
                        property.Name = propertyInfo.Name;

                        var propertyType = propertyInfo.PropertyType;
                        property.Nullable = propertyType.TryUnwrapNullableType(out propertyType) || !propertyType.IsValueType();

                        PrimitiveTypeKind primitiveTypeKind;
                        if (EdmConstants.TryGetPrimitiveTypeKind(propertyType, out primitiveTypeKind))
                        {
                            property.TypeUsage = TypeUsage.Create(EdmConstants.GetPrimitiveType(primitiveTypeKind));
                        }
                        entitySet.ElementType.AddProperty(property);
                    }

                    var typeToLoad = type;
                    do
                    {
                        typeToLoad = typeToLoad.BaseType();
                    }
                    while (typeToLoad != null && typeToLoad != typeof(Object));

                    if (typeToLoad == null || typeToLoad == typeof(Object))
                    {
                        typeToLoad = type;
                    }

                    _entitySetMappingsCache[type] = new EntitySetTypePair(entitySet, typeToLoad);
                }
            }
        }

        private static IEnumerable<PropertyInfo> GetProperties(Type type, bool declaredOnly, IEnumerable<Type> knownTypes = null, bool includePrivate = false)
        {
            knownTypes = knownTypes ?? Enumerable.Empty<Type>();

            var propertyInfos
                = from p in declaredOnly ? type.GetDeclaredProperties() : type.GetNonHiddenProperties()
                  where !p.IsStatic() && p.IsValidStructuralProperty()
                  let m = p.Getter()
                  where (includePrivate || (m.IsPublic || knownTypes.Contains(p.PropertyType)))
                        && (!declaredOnly || type.BaseType().GetInstanceProperties().All(bp => bp.Name != p.Name))
                        && ((!IsEnumType(p.PropertyType) && !IsSpatialType(p.PropertyType)))
                        && !p.PropertyType.IsNested
                  select p;
            return propertyInfos;
        }

        private static bool IsEnumType(Type type)
        {
            type.TryUnwrapNullableType(out type);

            return type.IsEnum();
        }

        private static bool IsSpatialType(Type type)
        {
            type.TryUnwrapNullableType(out type);

            return false;
        }
    }
}
