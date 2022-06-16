using System.Collections.Generic;

namespace Rundo.RuntimeEditor.Behaviours
{
    public enum CssPropertyEnum
    {
        LabelWidth,
    }
    
    public class CssData
    {
        public Dictionary<CssPropertyEnum, object> Values = new Dictionary<CssPropertyEnum, object>();

        public CssData SetValue(CssPropertyEnum property, object value)
        {
            Values[property] = value;
            return this;
        }

        public bool? GetBool(CssPropertyEnum property)
        {
            if (TryGetBool(property, out var value))
                return value;

            return null;
        }

        public bool TryGetBool(CssPropertyEnum property, out bool value)
        {
            value = false;
            if (TryGet(property, out var obj))
            {
                value = (bool)obj;
                return true;
            }

            return false;
        }

        public int? GetInt(CssPropertyEnum property)
        {
            if (TryGetInt(property, out var value))
                return value;

            return null;
        }

        public bool TryGetInt(CssPropertyEnum property, out int value)
        {
            value = 0;
            if (TryGet(property, out var obj))
            {
                value = (int)obj;
                return true;
            }

            return false;
        }

        public float? GetFloat(CssPropertyEnum property)
        {
            if (TryGetFloat(property, out var value))
                return value;

            return null;
        }

        public bool TryGetFloat(CssPropertyEnum property, out float value)
        {
            value = 0f;
            if (TryGet(property, out var obj))
            {
                value = (float)obj;
                return true;
            }

            return false;
        }

        public string GetString(CssPropertyEnum property)
        {
            if (TryGetValue(property, out var value))
                return (string)value;

            return null;
        }
        
        public bool TryGetString(CssPropertyEnum property, out string value)
        {
            value = "";
            if (TryGet(property, out var obj))
            {
                value = (string)obj;
                return true;
            }

            return false;
        }

        public T? Get<T>(CssPropertyEnum property) where T: struct
        {
            if (TryGet<T>(property, out var value))
                return value;

            return null;
        }

        public bool TryGet<T>(CssPropertyEnum property, out T value) where T: struct
        {
            value = default;
            if (TryGet(property, out var obj))
            {
                value = (T)obj;
                return true;
            }

            return false;
        }

        public object Get(CssPropertyEnum property)
        {
            if (TryGet(property, out var value))
                return value;

            return null;
        }

        public bool TryGet(CssPropertyEnum property, out object value)
        {
            value = default;
            if (TryGetValue(property, out object obj))
            {
                value = obj;
                return true;
            }

            return false;
        }

        public bool TryGetValue(CssPropertyEnum property, out object value)
        {
            return Values.TryGetValue(property, out value);
        }
    }
}


