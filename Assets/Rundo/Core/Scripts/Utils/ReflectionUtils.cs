using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Rundo.Core.Data;

namespace Rundo.Core.Utils
{
    public static class ReflectionUtils
    {
        private static readonly Dictionary<Type, Dictionary<string, MemberInfo>> MemberInfoCache =
            new Dictionary<Type, Dictionary<string, MemberInfo>>();
        private static readonly Dictionary<Type, bool> IsListCache = new Dictionary<Type, bool>();

        public enum UiTypeEnum
        {
            Primitive,
            Object,
            List,
        }

        public static bool IsList(Type type)
        {
            if (type.IsGenericType == false)
                return false;
            
            if (IsListCache.TryGetValue(type, out var isList))
                return isList;

            isList = typeof(List<>).IsAssignableFrom(type.GetGenericTypeDefinition());
            IsListCache[type] = isList;
            return isList;
        }

        public static Type GetListType(Type type)
        {
            if (IsList(type))
                return type.GetGenericArguments()[0];
            throw new Exception($"Cannot get list type if type {type.Name} is not list");
        }
    
        public static UiTypeEnum GetUiType(Type type)
        {
            if (type == typeof(int))
                return UiTypeEnum.Primitive;
            if (type == typeof(float))
                return UiTypeEnum.Primitive;
            if (type == typeof(bool))
                return UiTypeEnum.Primitive;
            if (typeof(ITEnum).IsAssignableFrom(type) || type.IsEnum)
                return UiTypeEnum.Primitive;
            if (typeof(IList).IsAssignableFrom(type))
                return UiTypeEnum.List;
            if (type.IsGenericType &&
                typeof(IList<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
                return UiTypeEnum.List;
            return UiTypeEnum.Object;
        }

        public static bool IsValueTypeButNotPrimitive(Type type)
        {
            return type.IsValueType && type.IsPrimitive == false;
        }

        public static object GetValue(object obj, MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo fieldInfo)
                return fieldInfo.GetValue(obj);
            if (memberInfo is PropertyInfo propertyInfo)
                return propertyInfo.GetValue(obj);
            return null;
        }

        public static object GetValue(object obj, string name)
        {
            return GetValue(obj, GetMemberInfo(obj.GetType(), name));
        }

        public static void SetValue(object obj, MemberInfo memberInfo, object value)
        {
            var memberType = GetMemberType(memberInfo);
            
            // implicit conversions
            
            // string -> enum
            if (value is string str && memberType.IsEnum)
                value = EnumUtils.Parse(memberType, str);
            
            if (memberInfo is FieldInfo fieldInfo)
                fieldInfo.SetValue(obj, value);
            else if (memberInfo is PropertyInfo propertyInfo)
                propertyInfo.SetValue(obj, value);
        }

        public static void SetValue(object obj, string name, object value)
        {
            SetValue(obj, GetMemberInfo(obj.GetType(), name), value);
        }

        public static Type GetMemberType(MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo fieldInfo)
                return fieldInfo.FieldType;
            if (memberInfo is PropertyInfo propertyInfo)
                return propertyInfo.PropertyType;
            return null;
        }

        public static MemberInfo GetMemberInfo(Type type, string name)
        {
            MemberInfo GetMemberInfoInternal()
            {
                foreach (var memberInfo in type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    if (memberInfo.Name == name)
                        return memberInfo;
                return null;
            }
            
            if (MemberInfoCache.TryGetValue(type, out var cache))
            {
                if (cache.TryGetValue(name, out var memberInfo))
                    return memberInfo;

                cache[name] = GetMemberInfoInternal();
                return cache[name];
            }

            MemberInfoCache[type] = new Dictionary<string, MemberInfo>
            {
                [name] = GetMemberInfoInternal()
            };

            return MemberInfoCache[type][name];
        }

        public static MethodInfo GetMethod(Type type, string name)
        {
            return type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static bool HasAttribute<T>(MemberInfo memberInfo) where T: Attribute
        {
            return memberInfo.GetCustomAttribute<T>() != null;
        }
    }
}

