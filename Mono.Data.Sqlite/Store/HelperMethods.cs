using System;
using System.Linq;
using System.Reflection;

namespace Mono.Data.Sqlite
{
    public static class HelperMethods
    {
#if NETFX_CORE
        public static object[] GetCustomAttributes(this Type type, Type attributeType, bool inherit) {
            return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
        }
        public static PropertyInfo[] GetProperties(this Type type) {
            return type.GetTypeInfo().DeclaredProperties.ToArray();
        }
        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }
        public static Type GetUnderlyingSystemType(this Type type)
        {
            return type;
        }
#else
        public static bool IsEnum(this Type type)
        {
            return type.IsEnum;
        }
        public static Type GetUnderlyingSystemType(this Type type)
        {
            return type.UnderlyingSystemType;
        }
#endif
    }
}
