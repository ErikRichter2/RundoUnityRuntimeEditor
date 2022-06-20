using System;
using UnityEngine.Assertions;

namespace Rundo.Core.Data
{
    [Serializable]
    public struct TString<T> : IStronglyTypedValue
    {
        private string ValueRaw;

        [StronglyTypedValueJsonGetter]
        [StronglyTypedValueJsonSetter]
        private string Value
        { 
            get => ValueRaw;
            set
            {
                Validate(value);
                ValueRaw = value;
            }
        }

        private bool AssertNotNull => typeof(IStronglyTypedValueIsNotNull).IsAssignableFrom(typeof(T));
        private bool AssertNotEmpty => typeof(IStronglyTypedStringIsNotEmpty).IsAssignableFrom(typeof(T));

        public TString(string value)
        {
            ValueRaw = null;
            Value = value;
            Validate(value);
        }

        private void Validate(string value)
        {
            if (AssertNotNull)
                Assert.IsNotNull(value);
            
            if (AssertNotEmpty)
                Assert.IsTrue(value != "");
                
            if (typeof(IStronglyTypedValueValidator<string>).IsAssignableFrom(typeof(T)))
                Assert.IsTrue(
                    ((IStronglyTypedValueValidator<string>)Activator.CreateInstance(typeof(T))).Validate(value));            
        }
        
        public static bool operator ==(TString<T> obj1, TString<T> obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(TString<T> obj1, TString<T> obj2)
        {
            return !(obj1 == obj2);
        }
        
        public override bool Equals(object obj)
        {
            if (obj is TString<T> id)
                return Equals(id);
            return false;
        }

        public bool Equals(TString<T> other)
        {
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        
        public override string ToString()
        {
            return $"Id: {Value}, Type: {typeof(T).Name}";
        }

        public string ToStringRawValue()
        {
            return Value;
        }

        public bool IsNullOrEmpty => string.IsNullOrEmpty(Value);
    }
}

