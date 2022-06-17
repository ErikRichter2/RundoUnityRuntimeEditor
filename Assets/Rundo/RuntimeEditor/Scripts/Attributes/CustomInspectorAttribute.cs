using System;

namespace Rundo.RuntimeEditor.Attributes
{
    /// <summary>
    /// Marks class as a custom inspector handler for specified type.
    /// </summary>
    public class CustomInspectorAttribute : Attribute
    {
        public readonly Type Type;
        
        public CustomInspectorAttribute(Type type)
        {
            Type = type;
        }
    }
}

