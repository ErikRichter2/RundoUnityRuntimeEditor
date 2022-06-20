using System;
using Newtonsoft.Json;

namespace Rundo.Core.Data
{
    public class DataCollectionReadJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return typeof(IDataCollection).IsAssignableFrom(objectType);
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

            var collection = (IDataCollection)RundoEngine.DataFactory.Instantiate(objectType);
            serializer.Populate(reader, collection);
            return collection;
        }
    }
}

