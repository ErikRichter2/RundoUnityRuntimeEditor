using System;

namespace Rundo.Core.Data
{
    public struct DataReference<T> : IDataReference
    {
        private DataReferenceValueWrapper<T> _valueWrapper;
        
        public void SetExistingDataReferenceValueWrapper(DataReferenceValueWrapper value)
        {
            if (value is DataReferenceValueWrapper<T> t)
                _valueWrapper = t;
        }
        
        public DataReferenceValueWrapper GetDataReferenceValueWrapper()
        {
            return _valueWrapper;
        }
        
        public DataReferenceValueWrapper<T> ValueWrapper
        {
            get
            {
                if (_valueWrapper == null)
                    _valueWrapper = (DataReferenceValueWrapper<T>)RundoEngine.DataFactory.Instantiate(typeof(DataReferenceValueWrapper<T>));
                return _valueWrapper;
            }
        }
        
        public T SerializedData => IsNull ? default : ValueWrapper.Data;
        
        public bool IsNull => _valueWrapper?.IsNull ?? true;

        public T Set(T serializedData)
        {
            if (serializedData == null)
            {
                ValueWrapper.Set(default);
                return default;
            }
            
            ValueWrapper.Set(serializedData);
            return serializedData;
        }

        public void SetJsonValue(object value)
        {
            Set((T)value);
        }

        public object GetJsonValue()
        {
            if (IsNull)
                return null;
            
            return ValueWrapper.Data;
        }
    }
}

