using System;

namespace Rundo.RuntimeEditor.Attributes
{
    /// <summary>
    /// Marks class as a custom inspector handler for specified type.
    /// </summary>
    public class CustomInspectorAttribute : Attribute
    {
        public readonly Type Type;
        public readonly string HeaderName;
        public readonly bool ShowHeader;
        
        public CustomInspectorAttribute(Type type, bool showHeader = true, string headerName = null)
        {
            Type = type;
            HeaderName = headerName;
            ShowHeader = showHeader;
        }
    }
}

