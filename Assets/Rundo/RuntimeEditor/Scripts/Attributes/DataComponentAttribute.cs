using System;

namespace Rundo.Ui
{
    /// <summary>
    /// This class can be used within a DataComponent wrapper and class data is therefore implicitly serialized. 
    /// </summary>
    public class DataComponentAttribute : Attribute
    {
        public readonly Type BehaviourType;
        
        public DataComponentAttribute() {}

        public DataComponentAttribute(Type behaviourType)
        {
            BehaviourType = behaviourType;
        }
    }
}

