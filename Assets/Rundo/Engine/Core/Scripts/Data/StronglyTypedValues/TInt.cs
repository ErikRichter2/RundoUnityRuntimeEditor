using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine.Assertions;

namespace Rundo.Core.Data
{
    [Serializable]
    public struct TInt<T> : IStronglyTypedValue
    {
        private int ValueRaw;

        [StronglyTypedValueJsonGetter]
        private int Value
        {
            get => ValueRaw;
            set
            {
                if (typeof(IStronglyTypedValueValidator<int>).IsAssignableFrom(typeof(T)))
                    Assert.IsTrue(
                        ((IStronglyTypedValueValidator<int>) Activator.CreateInstance(typeof(T))).Validate(value));

                ValueRaw = value;
            }
        }

        [StronglyTypedValueJsonSetter]
        private object SetValueFromJson
        {
            set
            {
                if (typeof(IStronglyTypedValueIsNotNull).IsAssignableFrom(typeof(T)))
                    Assert.IsNotNull(value);

                Value = value == null ? 0 : (int) ((long) value);
            }
        } 
        

        public TInt(int value)
        {
            ValueRaw = 0;
            Value = value;
        }

        public TInt(string value, bool silent = false)
        {
            ValueRaw = 0;

            int parsed = 0;
            try
            {
                parsed = int.Parse(value);
            }
            catch (Exception e)
            {
                if (silent == false)
                    throw new Exception(e.Message);
            }

            Value = parsed;
        }

        public static TInt<T> operator +(TInt<T> obj1, TInt<T> obj2)
        {
            return new TInt<T>(obj1.ToIntRawValue() + obj2.ToIntRawValue());
        }
        
        public static bool operator ==(TInt<T> obj1, TInt<T> obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(TInt<T> obj1, TInt<T> obj2)
        {
            return !(obj1 == obj2);
        }
        
        public override bool Equals(object obj)
        {
            if (obj is TInt<T> id)
                return Equals(id);
            return false;
        }

        public bool Equals(TInt<T> other)
        {
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        [JsonIgnore] public bool IsNegative => !IsPositive && !IsZero;
        [JsonIgnore] public bool IsZero => Value == 0;
        [JsonIgnore] public bool IsPositive => Value > 0;

        public override string ToString()
        {
            return $"Id: {Value}, Type: {typeof(T).Name}";
        }
        
        [Pure]
        public int ToIntRawValue()
        {
            return Value;
        }

        public string ToStringRawValue()
        {
            return Value.ToString();
        }
    }
}

