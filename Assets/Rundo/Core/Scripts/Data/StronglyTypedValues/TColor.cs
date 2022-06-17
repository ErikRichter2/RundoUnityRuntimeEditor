using System;
using UnityEngine;

namespace Rundo.Core.Data
{
    [Serializable]
    public struct TColor : IStronglyTypedValue
    {
        private string _valueRaw;

        [StronglyTypedValueJsonGetter]
        [StronglyTypedValueJsonSetter]
        private string Value
        { 
            get => _valueRaw;
            set => _valueRaw = value;
        }

        public TColor(string value)
        {
            _valueRaw = value;
        }

        public TColor(Color value)
        {
            _valueRaw = ColorToHTML(value);
        }
        
        public static string ColorToHTML(Color c)
        {
            return $"#{ColorUtility.ToHtmlStringRGBA(c)}";
        }
        
        public static Color HTMLToColor(string htmlColor)
        {
            if (string.IsNullOrEmpty(htmlColor) == false && htmlColor[0] != '#')
                htmlColor = "#" + htmlColor;
            
            var res = Color.white;
            if (ColorUtility.TryParseHtmlString(htmlColor, out var c))
                res = c;
            return res;
        }


        public static implicit operator Color(TColor value) => HTMLToColor(value.Value);
        public static implicit operator TColor(Color value) => new TColor(value);

        public static bool operator ==(TColor obj1, TColor obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(TColor obj1, TColor obj2)
        {
            return !(obj1 == obj2);
        }
        
        public override bool Equals(object obj)
        {
            if (obj is TColor id)
                return Equals(id);
            return false;
        }

        public bool Equals(TColor other)
        {
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        
        public override string ToString()
        {
            return $"Color: {Value}";
        }

        public string ToStringRawValue()
        {
            return Value;
        }
    }
}

