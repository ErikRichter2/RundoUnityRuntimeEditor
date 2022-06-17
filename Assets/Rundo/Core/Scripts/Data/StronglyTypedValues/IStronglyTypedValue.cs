using System;

namespace Rundo.Core.Data
{
    /**
     * Marks field/property that is used in the Json read as a value setter
     */
    [AttributeUsage(AttributeTargets.Property)]
    public class StronglyTypedValueJsonSetterAttribute : Attribute
    {
        
    }

    /**
     * Marks field/property that is used in the Json write as a value getter
     */
    [AttributeUsage(AttributeTargets.Property)]
    public class StronglyTypedValueJsonGetterAttribute : Attribute
    {
        
    }

    /**
     * Used for Json serialization - result of CanConvert
     */
    public interface IStronglyTypedValue
    {
        string ToStringRawValue();
    }

    public interface IStronglyTypedValueValidator<in T>
    {
        bool Validate(T value);
    }

    public interface IStronglyTypedValueIsNotNull
    {
        
    }

    public interface IStronglyTypedStringIsNotEmpty
    {
        
    }
}

