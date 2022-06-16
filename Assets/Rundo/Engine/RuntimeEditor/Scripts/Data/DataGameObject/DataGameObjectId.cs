using System;
using Rundo.Core.Data;

namespace Rundo.RuntimeEditor.Data
{
    public struct DataGameObjectId : IGuid
    {
        private string ValueRaw;

        [StronglyTypedValueJsonGetter]
        [StronglyTypedValueJsonSetter]
        private string Value
        { 
            get => ValueRaw;
            set => ValueRaw = value;
        }

        public static DataGameObjectId Create(string guid = null)
        {
            var res = new DataGameObjectId();
            if (string.IsNullOrEmpty(guid))
                res.CreateNewGUID();
            else
                res.Value = guid;
            return res;
        }

        public IGuid ReturnNewGUID()
        {
            return Create();
        }

        public bool IsGuidOfType(Type type)
        {
            return type.IsAssignableFrom(typeof(DataGameObjectId));
        }

        public void SetGUID(string guid)
        {
            Value = guid;
        }

        public void CreateNewGUID()
        {
            Value = Guid.NewGuid().ToString();
        }

        public static bool operator ==(DataGameObjectId obj1, DataGameObjectId obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(DataGameObjectId obj1, DataGameObjectId obj2)
        {
            return !(obj1 == obj2);
        }
        
        public override bool Equals(object obj)
        {
            if (obj is IGuid id)
                return Equals(id);
            return false;
        }

        public bool Equals(IGuid other)
        {
            return Value == other.ToStringRawValue();
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        
        public override string ToString()
        {
            return Value;
        }

        public string ToStringRawValue()
        {
            return Value;
        }

        public bool IsNullOrEmpty => string.IsNullOrEmpty(Value);
    }

}

