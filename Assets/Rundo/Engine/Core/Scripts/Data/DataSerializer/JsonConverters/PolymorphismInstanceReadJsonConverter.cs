using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rundo.Core.Data
{
    public class PolymorphismInstanceReadJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return typeof(IPolymorphismBase).IsAssignableFrom(objectType);
        }

#nullable enable
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
#nullable disable
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            
            var jObject = JObject.Load(reader);
            var instance = RundoEngine.DataFactory.Instantiate(objectType, jObject, null);
            if (instance != null)
                RundoEngine.DataSerializer.Populate(serializer, jObject, instance);

            return instance;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}

