using System;
using System.Collections;
using System.Linq;

namespace Bnf.Serialization.Infrastructure
{
    public class TypeHelper
    {
        public static bool IsEnumerable(Type type)
        {
            if (type == null)
                return false;

            return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static bool IsPrimitive(Type type)
        {
            if (type == null)
                return false;

            return !type.IsClass || type == typeof(string);
        }

        public static Type GetElementType(Type type)
        {
            var itemType = type.IsGenericType ? type.GetGenericArguments()?.FirstOrDefault() : type.GetElementType();
            if (itemType == null)
                itemType = type.BaseType.IsGenericType ? type.BaseType.GetGenericArguments()?.FirstOrDefault() : type.BaseType.GetElementType();

            return itemType;
        }

        public static object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }
    }
}
