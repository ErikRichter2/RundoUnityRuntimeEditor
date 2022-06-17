using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Rundo.Core.Data
{
    public interface ITEnum
    {
        public string ToStringRawValue();
        public TEnumType ToEnumRawValue<TEnumType>();
        public void SetValueDynamic(string value);
    }
    
    [Serializable]
    public struct TEnum<T> : IStronglyTypedValue, ITEnum where T: Enum
    {
        private object _unparsedValue;
        private bool _wasParsed;
        private T _valueRaw;

        [StronglyTypedValueJsonGetter]
        private int GetValueToJson => (int)(object)ParsedValue;

        [StronglyTypedValueJsonSetter]
        private object SetValueFromJson
        {
            set
            {
                Assert.IsNotNull(value);
                _wasParsed = false;
                _unparsedValue = value;
            }
        }

        public void SetValueDynamic(string value)
        {
            var parsed = (T)Enum.Parse(typeof(T), value);
            SetEnumRawValue(parsed);
        }

        public TEnum(T value)
        {
            _valueRaw = value;
            _wasParsed = true;
            _unparsedValue = null;
        }

        public TEnum(string value)
        {
            _valueRaw = (T) Enum.Parse(typeof(T), value, false);
            _wasParsed = true;
            _unparsedValue = null;
        }
        
        public TEnum(int value)
        {
            _valueRaw = (T) Enum.ToObject(typeof(T), value);
            _wasParsed = true;
            _unparsedValue = null;
        }

        private T ParsedValue
        {
            get
            {
                if (_wasParsed)
                    return _valueRaw;
                
                _wasParsed = true;
                if (_unparsedValue != null)
                {
                    if (_unparsedValue is string s)
                        _valueRaw = (T)Enum.Parse(typeof(T), s, true);
                    else
                    {
                        var valueInt = Convert.ToInt32(_unparsedValue);
                        var valueObj = (object) valueInt;
                        _valueRaw = (T) valueObj;
                    }
                }
                
                return _valueRaw;
            }
        }
        
        public static implicit operator TEnum<T>(T value) => new TEnum<T>(value);
        public static implicit operator T(TEnum<T> value) => value.ParsedValue;

        public void SetEnumRawValue(T value)
        {
            _wasParsed = true;
            _valueRaw = value;
        }

        public static bool operator ==(TEnum<T> obj1, TEnum<T> obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(TEnum<T> obj1, TEnum<T> obj2)
        {
            return !(obj1 == obj2);
        }
        
        public override bool Equals(object obj)
        {
            if (obj is T rawEnum)
                return Equals(rawEnum);
            if (obj is TEnum<T> stronglyTypedEnum)
                return Equals(stronglyTypedEnum);
            return false;
        }

        public bool Equals(TEnum<T> other)
        {
            return Equals(other.ParsedValue);
        }

        public bool Equals(T other)
        {
            return EqualityComparer<T>.Default.Equals(ParsedValue, other);
        }

        public override int GetHashCode()
        {
            return ParsedValue.GetHashCode();
        }
        
        public override string ToString()
        {
            return $"Id: {ParsedValue}, Type: {typeof(T).Name}";
        }

        public string ToStringRawValue()
        {
            return ParsedValue.ToString();
        }
        
        public T ToEnumRawValue()
        {
            return ParsedValue;
        }

        public TEnumType ToEnumRawValue<TEnumType>()
        {
            if (ParsedValue is TEnumType t)
                return t;

            throw new Exception($"Expected type is {typeof(TEnumType).Name}, declared type is {typeof(T).Name}");
        }

        public int ToIntRawValue()
        {
            return (int)(object)ParsedValue;
        }
    }

}

