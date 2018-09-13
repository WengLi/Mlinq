using System.Reflection;
using System.Linq;

namespace Mlinq.Common
{
    internal static class PropertyInfoExtensions
    {
        public static bool IsStatic(this PropertyInfo property)
        {
            return (property.Getter() ?? property.Setter()).IsStatic;
        }

        public static bool IsPublic(this PropertyInfo property)
        {
            var getter = property.Getter();
            var getterAccess = getter == null ? MethodAttributes.Private : (getter.Attributes & MethodAttributes.MemberAccessMask);

            var setter = property.Setter();
            var setterAccess = setter == null ? MethodAttributes.Private : (setter.Attributes & MethodAttributes.MemberAccessMask);

            var propertyAccess = getterAccess > setterAccess ? getterAccess : setterAccess;

            return propertyAccess == MethodAttributes.Public;
        }

        public static MethodInfo Getter(this PropertyInfo property)
        {
            return property.GetMethod;
        }

        public static MethodInfo Setter(this PropertyInfo property)
        {
            return property.SetMethod;
        }

        public static bool IsValidStructuralProperty(this PropertyInfo propertyInfo)
        {
            return propertyInfo.IsValidInterfaceStructuralProperty() && !propertyInfo.Getter().IsAbstract;
        }

        public static bool IsValidInterfaceStructuralProperty(this PropertyInfo propertyInfo)
        {
            return propertyInfo.CanRead
                   && (propertyInfo.CanWriteExtended() || propertyInfo.PropertyType.IsCollection())
                   && propertyInfo.GetIndexParameters().Length == 0
                   && propertyInfo.PropertyType.IsValidStructuralPropertyType();
        }

        public static bool CanWriteExtended(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.CanWrite)
            {
                return true;
            }

            var declaredProperty = GetDeclaredProperty(propertyInfo);
            return declaredProperty != null && declaredProperty.CanWrite;
        }

        private static PropertyInfo GetDeclaredProperty(PropertyInfo propertyInfo)
        {
            return propertyInfo.DeclaringType == propertyInfo.ReflectedType
                       ? propertyInfo
                       : propertyInfo.DeclaringType
                             .GetInstanceProperties()
                             .SingleOrDefault(
                                 p => p.Name == propertyInfo.Name
                                      && !p.GetIndexParameters().Any()
                                      && p.PropertyType == propertyInfo.PropertyType);
        }
    }
}
