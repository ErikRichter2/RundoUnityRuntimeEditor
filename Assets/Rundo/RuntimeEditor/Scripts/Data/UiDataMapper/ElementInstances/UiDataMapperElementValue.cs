using Rundo.Core.Data;

namespace Rundo.RuntimeEditor.Data.UiDataMapper
{
    /// <summary>
    /// Adds a UiDataMapper logic properties to a raw value of TData type - this raw value is handled by an UI element
    /// </summary>
    /// <typeparam name="TData">Type of value handled by an UI element</typeparam>
    public struct UiDataMapperElementValue<TValue> : IDataSetValue
    {
        public object ValueDynamic => Value;
        public int DataIndex => MetaData.DataIndex;
        public int? ListIndex => MetaData.ListIndex;
        public bool IgnoreUndoRedo => MetaData.IgnoreUndoRedo;
        
        
        
        
        public TValue Value;
        public UiDataMapperElementValueMetaData MetaData;

        public UiDataMapperElementValue(TValue value)
        {
            Value = value;
            MetaData = default;
        }
        
        public UiDataMapperElementValue(TValue value, UiDataMapperElementValueMetaData metaData)
        {
            Value = value;
            MetaData = metaData;
        }

        public UiDataMapperElementValue<object> ToDynamic()
        {
            return new UiDataMapperElementValue<object>(Value, MetaData);
        }

        public static UiDataMapperElementValue<object> ToDynamic(object value, UiDataMapperElementValueMetaData metaData)
        {
            return new UiDataMapperElementValue<object>(value, metaData);
        }
    }    
}
