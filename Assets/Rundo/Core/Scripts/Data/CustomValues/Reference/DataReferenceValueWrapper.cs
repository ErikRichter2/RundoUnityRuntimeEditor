using Newtonsoft.Json;

namespace Rundo.Core.Data
{
    public class DataReferenceValueWrapper
    {
        [JsonIgnore]
        protected object _data;
    }

    public class DataReferenceValueWrapper<T> : DataReferenceValueWrapper
    {
        [JsonIgnore] public T Data => (T)_data;

        private DataReferenceValueWrapper() {}
        
        [JsonIgnore]
        public bool IsNull => _data == default;
        
        [JsonIgnore]
        public bool IsSet => IsNull == false;
        
        public void Set(T data)
        {
            _data = data;
        }
        
        public static implicit operator T(DataReferenceValueWrapper<T> value) => value.Data;
    }
}

