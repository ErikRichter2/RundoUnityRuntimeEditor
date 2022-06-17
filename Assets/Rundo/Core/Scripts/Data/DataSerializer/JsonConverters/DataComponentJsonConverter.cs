using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rundo.RuntimeEditor.Data
{
    public class DataComponentReadJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return typeof(DataComponent).IsAssignableFrom(objectType);
        }

#nullable enable
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
#nullable disable
        {
            throw new NotImplementedException();
        }

#nullable enable
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
#nullable disable
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var jObject = JObject.Load(reader);
            
            var dataComponent = (DataComponent)RundoEngine.DataFactory.Instantiate(objectType, jObject, null);
            RundoEngine.DataSerializer.Populate(serializer, jObject, dataComponent);
            return dataComponent;
        }     
    }
}

