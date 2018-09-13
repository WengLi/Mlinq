using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mlinq.Common
{
    internal static class TypeExtensions
    {
        internal static readonly MethodInfo GetDefaultMethod = typeof(TypeExtensions).GetOnlyDeclaredMethod("GetDefault");

        private static T GetDefault<T>()
        {
            return default(T);
        }

        internal static MethodInfo GetDeclaredMethod(this Type type, string name, params Type[] parameterTypes)
        {
            return type.GetDeclaredMethods(name).SingleOrDefault(m => m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));
        }

        internal static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type, string name)
        {
            return type.GetTypeInfo().GetDeclaredMethods(name);
        }

        internal static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type)
        {
            return type.GetTypeInfo().DeclaredMethods;
        }

        internal static MethodInfo GetOnlyDeclaredMethod(this Type type, string name)
        {
            return type.GetDeclaredMethods(name).SingleOrDefault();
        }

        internal static IEnumerable<PropertyInfo> GetInstanceProperties(this Type type)
        {
            return type.GetRuntimeProperties().Where(p => !p.IsStatic());
        }

        internal static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        internal static bool ContainsGenericParameters(this Type type)
        {
            return type.GetTypeInfo().ContainsGenericParameters;
        }

        internal static bool IsValueType(this Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        internal static bool IsInterface(this Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }

        internal static bool IsPrimitive(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive;
        }

        internal static bool IsGenericTypeDefinition(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition;
        }

        public static bool IsGenericParameter(this Type type)
        {
            return type.GetTypeInfo().IsGenericParameter;
        }

        internal static Assembly Assembly(this Type type)
        {
            return type.GetTypeInfo().Assembly;
        }

        internal static Type BaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

        internal static string NestingNamespace(this Type type)
        {
            if (!type.IsNested)
            {
                return type.Namespace;
            }

            var fullName = type.FullName;

            return fullName.Substring(0, fullName.Length - type.Name.Length - 1).Replace('+', '.');
        }

        internal static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        internal static PropertyInfo GetDeclaredProperty(this Type type, string name)
        {
            return type.GetTypeInfo().GetDeclaredProperty(name);
        }

        internal static bool IsValidStructuralType(this Type type)
        {
            return !(type.IsGenericType()
                     || type.IsValueType()
                     || type.IsPrimitive()
                     || type.IsInterface()
                     || type.IsArray
                     || type == typeof(string)
                     || type.IsGenericTypeDefinition()
                     || type.IsPointer
                     || type == typeof(object));
        }

        internal static bool IsValidStructuralPropertyType(this Type type)
        {
            return !(type.IsGenericTypeDefinition()
                     || type.IsPointer
                     || type == typeof(object));
        }

        internal static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type type)
        {
            return type.GetTypeInfo().DeclaredProperties;
        }

        internal static IEnumerable<PropertyInfo> GetNonHiddenProperties(this Type type)
        {
            return from property in type.GetRuntimeProperties()
                   group property by property.Name
                       into propertyGroup
                       select MostDerived(propertyGroup);
        }

        private static PropertyInfo MostDerived(IEnumerable<PropertyInfo> properties)
        {
            PropertyInfo mostDerivedProperty = null;
            foreach (var property in properties)
            {
                if (mostDerivedProperty == null
                    || (mostDerivedProperty.DeclaringType != null
                    && mostDerivedProperty.DeclaringType.IsAssignableFrom(property.DeclaringType)))
                {
                    mostDerivedProperty = property;
                }
            }

            return mostDerivedProperty;
        }

        internal static bool IsCollection(this Type type)
        {
            return type.IsCollection(out type);
        }

        internal static bool IsCollection(this Type type, out Type elementType)
        {
            elementType = TryGetElementType(type, typeof(ICollection<>));

            if (elementType == null || type.IsArray)
            {
                elementType = type;
                return false;
            }

            return true;
        }

        internal static Type TryGetElementType(this Type type, Type interfaceOrBaseType)
        {
            if (!type.IsGenericTypeDefinition())
            {
                var types = GetGenericTypeImplementations(type, interfaceOrBaseType).ToList();

                return types.Count == 1 ? types[0].GetGenericArguments().FirstOrDefault() : null;
            }

            return null;
        }

        internal static IEnumerable<Type> GetGenericTypeImplementations(this Type type, Type interfaceOrBaseType)
        {
            if (!type.IsGenericTypeDefinition())
            {
                return (interfaceOrBaseType.IsInterface() ? type.GetInterfaces() : type.GetBaseTypes())
                    .Union(new[] { type })
                    .Where(
                        t => t.IsGenericType()
                             && t.GetGenericTypeDefinition() == interfaceOrBaseType);
            }

            return Enumerable.Empty<Type>();
        }

        internal static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            type = type.BaseType();

            while (type != null)
            {
                yield return type;

                type = type.BaseType();
            }
        }

        internal static bool TryUnwrapNullableType(this Type type, out Type underlyingType)
        {
            underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType != type;
        }

        internal static Type GetNonEnumElementType(this Type type)
        {
            var ienum = type.FindIEnumerable();
            if (ienum == null)
            {
                return type;
            }
            return ienum.GetGenericArguments()[0];
        }

        internal static Type FindIEnumerable(this Type seqType)
        {
            if (seqType == null || seqType == typeof(string) || seqType == typeof(byte[]))
            {
                return null;
            }
            if (seqType.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            }
            if (seqType.IsGenericType())
            {
                foreach (var arg in seqType.GetGenericArguments())
                {
                    var ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType))
                    {
                        return ienum;
                    }
                }
            }
            var ifaces = seqType.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (var iface in ifaces)
                {
                    var ienum = FindIEnumerable(iface);
                    if (ienum != null)
                    {
                        return ienum;
                    }
                }
            }
            if (seqType.BaseType() != null && seqType.BaseType() != typeof(object))
            {
                return FindIEnumerable(seqType.BaseType());
            }
            return null;
        }

        internal static Type GetNonNullableType(this Type type)
        {
            if (type != null)
            {
                return Nullable.GetUnderlyingType(type) ?? type;
            }
            return null;
        }

        internal static MemberInfo GetPropertyOrField(this MemberInfo member, out string name, out Type type)
        {
            name = null;
            type = null;
            if (member.MemberType == MemberTypes.Field)
            {
                var field = (FieldInfo)member;
                name = field.Name;
                type = field.FieldType;
                return field;
            }
            else if (member.MemberType == MemberTypes.Property)
            {
                var property = (PropertyInfo)member;
                if (0 != property.GetIndexParameters().Length)
                {
                    throw new NotSupportedException();
                }
                name = property.Name;
                type = property.PropertyType;
                return property;
            }
            else if (member.MemberType == MemberTypes.Method)
            {
                var method = (MethodInfo)member;
                if (method.IsSpecialName)
                {
                    foreach (var property in method.DeclaringType.GetRuntimeProperties())
                    {
                        if (property.CanRead && (property.Getter() == method))
                        {
                            return GetPropertyOrField(property, out name, out type);
                        }
                    }
                }
            }
            throw new NotSupportedException();
        }

        internal static object GetDefaultValue(this Type type)
        {
            if (!type.IsValueType() || (type.IsGenericType() && typeof(Nullable<>) == type.GetGenericTypeDefinition()))
            {
                return null;
            }
            var getDefaultMethod = GetDefaultMethod.MakeGenericMethod(type);
            var defaultValue = getDefaultMethod.Invoke(null, new object[] { });
            return defaultValue;
        }

        public static bool IsNullable(this Type type)
        {
            return !type.IsValueType() || Nullable.GetUnderlyingType(type) != null;
        }
    }
}
