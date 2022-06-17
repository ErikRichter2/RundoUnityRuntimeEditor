using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rundo.Core.Data
{
    public class MonoBehaviourWriteJsonConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return typeof(MonoBehaviour).IsAssignableFrom(objectType);
        }

#nullable enable
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
#nullable disable
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}

