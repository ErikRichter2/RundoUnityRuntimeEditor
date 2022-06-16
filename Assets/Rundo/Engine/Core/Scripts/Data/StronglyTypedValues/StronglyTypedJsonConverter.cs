using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Rundo.Core.Utils;

namespace Rundo.Core.Data
{
    public class StronglyTypedJsonConverter : JsonConverter
    {
        private static Dictionary<Type, MemberInfo> _jsonGetterCache = new Dictionary<Type, MemberInfo>();
        private static Dictionary<Type, MemberInfo> _jsonSetterCache = new Dictionary<Type, MemberInfo>();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jsonGetter = GetJsonGetter(GetUnderlyingTypeIfExists(value.GetType()));
            if (jsonGetter != null)
            {
                writer.WriteValue(ReflectionUtils.GetValue(value, jsonGetter));
                return;
            }
            
            throw new Exception($"StronglyTypedJsonConverter: missing raw value attribute [StronglyTypedValueJsonGetterAttribute] in the {value.GetType().Name}");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                if (objectType.IsClass || IsNullableType(objectType))
                    return existingValue;
            }
            
            var jsonSetter = GetJsonSetter(GetUnderlyingTypeIfExists(objectType));
            if (jsonSetter != null)
            {
                if (existingValue == null)
                    existingValue = Activator.CreateInstance(GetUnderlyingTypeIfExists(objectType));

                ReflectionUtils.SetValue(existingValue, jsonSetter, reader.Value);
                return existingValue;
            }
            
            throw new Exception($"StronglyTypedJsonConverter: missing raw value attribute [StronglyTypedValueJsonSetterAttribute] in the {existingValue.GetType().Name}");
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IStronglyTypedValue).IsAssignableFrom(GetUnderlyingTypeIfExists(objectType));
        }

        private static MemberInfo GetJsonGetter(Type structType)
        {
            if (_jsonGetterCache.TryGetValue(structType, out var memberInfo))
                return memberInfo;

            memberInfo = GetMemberWithAttribute<StronglyTypedValueJsonGetterAttribute>(structType);
            _jsonGetterCache.Add(structType, memberInfo);
            
            return memberInfo;
        }

        private static MemberInfo GetJsonSetter(Type structType)
        {
            if (_jsonSetterCache.TryGetValue(structType, out var memberInfo))
                return memberInfo;

            memberInfo = GetMemberWithAttribute<StronglyTypedValueJsonSetterAttribute>(structType);
            _jsonSetterCache.Add(structType, memberInfo);
            
            return memberInfo;
        }

        private static MemberInfo GetMemberWithAttribute<T>(Type valueType) where T: Attribute
        {
            var members = valueType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var member in members)
            {
                var att = member.GetCustomAttribute<T>();
                if (att != null)
                    return member;
            }

            return null;
        }

        private bool IsNullableType(Type objectType)
        {
            return Nullable.GetUnderlyingType(objectType) != null;
        }

        private Type GetUnderlyingTypeIfExists(Type objectType)
        {
            var nullableType = Nullable.GetUnderlyingType(objectType);
            return nullableType != null ? nullableType : objectType;
        }
    }
}

