using System;
using System.Globalization;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Rundo.Core.Data
{
    [Serializable]
    public struct TFloat<T> : IStronglyTypedValue
    {
        public static TFloat<T> Zero = new TFloat<T>(0);

        private float ValueRaw;

        [StronglyTypedValueJsonGetter]
        private float Value
        {
            get => ValueRaw;
            set => ValueRaw = value;
        }

        [StronglyTypedValueJsonSetter]
        private object SetValueFromJson
        {
            set => Value = value == null ? 0 : (float)(double)value;
        }


        public TFloat(float value)
        {
            ValueRaw = 0;
            Value = value;
        }

        public TFloat(string value, bool silent = false)
        {
            ValueRaw = 0;

            float parsed = 0;
            try
            {
                parsed = float.Parse(value, CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                if (silent == false)
                    throw new Exception(e.Message);
            }

            Value = parsed;
        }

        public static TFloat<T> operator +(TFloat<T> obj1, TFloat<T> obj2)
        {
            return new TFloat<T>(obj1.ToFloatRawValue() + obj2.ToFloatRawValue());
        }

        public static TFloat<T> operator -(TFloat<T> obj1, TFloat<T> obj2)
        {
            return new TFloat<T>(obj1.ToFloatRawValue() - obj2.ToFloatRawValue());
        }

        public static bool operator >(TFloat<T> obj1, TFloat<T> obj2)
        {
            return obj1.ToFloatRawValue() > obj2.ToFloatRawValue();
        }

        public static bool operator <(TFloat<T> obj1, TFloat<T> obj2)
        {
            return obj1.ToFloatRawValue() < obj2.ToFloatRawValue();
        }
        
        public static bool operator ==(TFloat<T> obj1, TFloat<T> obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(TFloat<T> obj1, TFloat<T> obj2)
        {
            return !(obj1 == obj2);
        }
        
        public override bool Equals(object obj)
        {
            if (obj is TFloat<T> id)
                return Equals(id);
            return false;
        }

        public bool Equals(TFloat<T> other)
        {
            return Math.Abs(Value - other.Value) < 0.000001f;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public TFloat<T> Round(int decimals)
        {
            return new TFloat<T>((float)Math.Round(ToFloatRawValue(), decimals));
        }

        [JsonIgnore] public bool IsNegative => !IsPositive && !IsZero;
        [JsonIgnore] public bool IsZero => Value == 0;
        [JsonIgnore] public bool IsPositive => Value > 0;

        public override string ToString()
        {
            return $"Id: {Value}, Type: {typeof(T).Name}";
        }
        
        [Pure]
        public float ToFloatRawValue()
        {
            return Value;
        }

        public string ToStringRawValue()
        {
            return Value.ToString();
        }
    }
}

