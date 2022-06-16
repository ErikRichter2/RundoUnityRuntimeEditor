using System;

namespace Rundo.Core.Data
{
    /// Marks the property to create instance of DataModel of explicit type - the type of the property. Keep
    /// in mind that the type has to extend DataModel<T> type where T is the class where this property is
    /// declared.
    [AttributeUsage(AttributeTargets.Property)]
    public class ExplicitModelAttribute : Attribute {}
}

