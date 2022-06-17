using System;
using Newtonsoft.Json.Serialization;

namespace Rundo.Core.Data
{
    public class DataTypeIdValueProvider : IValueProvider
    {
        private readonly string _dataTypeId;

        public DataTypeIdValueProvider(string dataTypeId)
        {
            _dataTypeId = dataTypeId;
        }

        public static JsonProperty CreateProperty(Type objectType, string dataTypeID)
        {
            var property = new JsonProperty();
            property.PropertyType = typeof(string);
            property.DeclaringType = objectType;
            property.Readable = true;
            property.Writable = true;
            property.ShouldSerialize = obj => true;
            property.PropertyName = nameof(IDataTypeId._dataTypeId);
            property.DefaultValue = dataTypeID;
            property.ValueProvider = new DataTypeIdValueProvider(dataTypeID);
            return property;
        }

#nullable enable
        public void SetValue(object target, object? value)
#nullable disable
        {
            
        }

#nullable enable
        public object? GetValue(object target)
#nullable disable
        {
            return _dataTypeId;
        }
    }
}

