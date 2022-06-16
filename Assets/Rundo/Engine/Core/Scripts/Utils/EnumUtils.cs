using System;
using System.Collections.Generic;

namespace Rundo.Core.Utils
{
    public static class EnumUtils
    {
        public static object Parse(Type enumType, string enumValue)
        {
            return Enum.Parse(enumType, enumValue);
        }
        
        public static IEnumerable<string> GetEnumValues(Type type)
        {
            return Enum.GetNames(type);
        }

        public static IEnumerable<T> GetEnumValues<T>()
        {
            return Enum.GetValues(typeof(T)) as IEnumerable<T>;
        }
    }
}
