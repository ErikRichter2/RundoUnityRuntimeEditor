using System;
using Newtonsoft.Json;
using UnityEngine.Assertions;

namespace Rundo.Core.Data
{
    public class DataReferenceWriteJsonConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return typeof(IDataReference).IsAssignableFrom(objectType);
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
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            
            Assert.IsTrue(value is IDataReference);
            serializer.Serialize(writer, ((IDataReference)value).GetJsonValue());
        }
    }
}

