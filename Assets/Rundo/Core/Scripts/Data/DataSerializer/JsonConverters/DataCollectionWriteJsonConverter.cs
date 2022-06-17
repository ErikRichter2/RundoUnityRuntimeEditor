using System;
using Newtonsoft.Json;
using UnityEngine.Assertions;

namespace Rundo.Core.Data
{
    public class DataCollectionWriteJsonConverter : JsonConverter
    {
        public override bool CanRead => false;
        
        public override bool CanConvert(Type objectType)
        {
            return typeof(IDataCollection).IsAssignableFrom(objectType);
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
            
            Assert.IsTrue(value is IDataCollection);

            writer.WriteStartArray();
            var collection = ((IDataCollection)value).GetValues();
            foreach (var it in collection)
                serializer.Serialize(writer, it);
            writer.WriteEndArray();
        }
    }
}

