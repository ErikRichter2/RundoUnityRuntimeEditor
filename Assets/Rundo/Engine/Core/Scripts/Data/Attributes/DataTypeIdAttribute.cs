using System;

namespace Rundo.Core.Data
{
    /// Provides json polymorphism
    /// 
    /// Binds a string id to a data type - this string id is implicitly serialized into a JSON (as a standalone property)
    /// and deserialization process then finds the final class marked with this id.
    [AttributeUsage(AttributeTargets.Class)]
    public class DataTypeIdAttribute : Attribute
    {
        public readonly string DataTypeId;
        
        public DataTypeIdAttribute(string typeId)
        {
            DataTypeId = typeId;
        }
    }
}

