using System;
using Rundo.Core.Data;

namespace Rundo.RuntimeEditor.Data
{
    public struct TDataComponentReference<TComponentType> : IDataComponentReference
    {
        private DataGameObjectId _dataGameObjectId;
        
        [StronglyTypedValueJsonGetter]
        private string JsonValueGetter
        {
            get => _dataGameObjectId.ToStringRawValue();
        }

        [StronglyTypedValueJsonSetter]
        private string JsonValueSetter
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                    _dataGameObjectId = default;
                else
                    _dataGameObjectId = DataGameObjectId.Create(value);
            }
        }

        public void SetDataGameObjectId(DataGameObjectId dataGameObjectId)
        {
            _dataGameObjectId = dataGameObjectId;
        } 

        public DataGameObject GetDataGameObject(IDataGameObjectFinder dataGameObjectFinder)
        {
            return dataGameObjectFinder.Find(_dataGameObjectId);  
        } 
        
        public IDataComponent<TComponentType> GetComponent(IDataGameObjectFinder dataGameObjectFinder)
        {
            return GetDataGameObject(dataGameObjectFinder)?.GetComponent<TComponentType>();
        } 

        private static TDataComponentReference<TComponentType> CreateFromDataComponent(DataComponent<TComponentType> value)
        {
            var res = new TDataComponentReference<TComponentType>();
            res.SetDataGameObjectId(value.DataGameObject.ObjectId);
            return res;
        }

        public static implicit operator TDataComponentReference<TComponentType>(DataComponent<TComponentType> value) =>
            CreateFromDataComponent(value);

        public bool IsOfType(Type type)
        {
            return type.IsAssignableFrom(typeof(TComponentType));
        }

        public static bool operator ==(TDataComponentReference<TComponentType> obj1, TDataComponentReference<TComponentType> obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(TDataComponentReference<TComponentType> obj1, TDataComponentReference<TComponentType> obj2)
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
            return _dataGameObjectId.ToStringRawValue() == other.ToStringRawValue();
        }

        public override int GetHashCode()
        {
            return _dataGameObjectId.GetHashCode();
        }
            
        public override string ToString()
        {
            return _dataGameObjectId.ToString();
        }

        public string ToStringRawValue()
        {
            return _dataGameObjectId.ToStringRawValue();
        }

        public void SetGUID(string guid)
        {
            _dataGameObjectId.SetGUID(guid);
        }

        public void CreateNewGUID()
        {
            _dataGameObjectId.CreateNewGUID();
        }

        public bool IsNullOrEmpty => _dataGameObjectId.IsNullOrEmpty;
        
        public bool IsGuidOfType(Type type)
        {
            return _dataGameObjectId.IsGuidOfType(type);
        }

        public IGuid ReturnNewGUID()
        {
            return _dataGameObjectId.ReturnNewGUID();
        }
    }
}

