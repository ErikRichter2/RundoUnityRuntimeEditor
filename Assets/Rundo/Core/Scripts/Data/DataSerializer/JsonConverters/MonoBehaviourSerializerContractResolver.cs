using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Object = UnityEngine.Object;

namespace Rundo.Core.Data
{
    public class MonoBehaviourSerializerContractResolver : DefaultContractResolver
    {
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var objectContract = base.CreateObjectContract(objectType);

            var finalTypeId =
                RundoEngine.ReflectionService.GetSerializedDataTypeIdByType(objectType);

            if (string.IsNullOrEmpty(finalTypeId) == false)
                objectContract.Properties.Add(DataTypeIdValueProvider.CreateProperty(objectType, finalTypeId));

            return objectContract;
        }
        
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (typeof(Object).IsAssignableFrom(member.ReflectedType))
                if (RundoEngine.ReflectionService.IsMonoBehaviourMember(member.Name))
                    property.Ignored = true;

            return property;
        }
    }
}

