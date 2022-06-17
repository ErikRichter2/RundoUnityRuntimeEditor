using System;
using Newtonsoft.Json;

namespace Rundo.Core.Data
{
    public class DataReferenceReadJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return typeof(IDataReference).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

#nullable enable
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
#nullable disable
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var instance = RundoEngine.DataFactory.Instantiate(objectType, null, null);
            IDataReference serializedDataReference = (IDataReference)Activator.CreateInstance(objectType, true);
            serializedDataReference.SetJsonValue(instance);
            return serializedDataReference;
        }     
    }
}

