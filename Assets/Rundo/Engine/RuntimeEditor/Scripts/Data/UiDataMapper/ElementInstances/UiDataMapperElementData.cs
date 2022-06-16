namespace Rundo.RuntimeEditor.Data.UiDataMapper
{
    /// <summary>
    /// Adds a UiDataMapper logic properties to a raw value of TData type
    /// </summary>
    /// <typeparam name="TData">Type of data to which is UiDataMapper mapped</typeparam>
    public struct UiDataMapperElementData<TData>
    {
        public TData Data;
        public UiDataMapperElementDataMetaData MetaData;

        public UiDataMapperElementData(TData data)
        {
            Data = data;
            MetaData = default;
        }

        public UiDataMapperElementData(TData data, UiDataMapperElementDataMetaData metaData)
        {
            Data = data;
            MetaData = metaData;
        }

        public UiDataMapperElementData<object> ToDynamic()
        {
            return new UiDataMapperElementData<object>(Data, MetaData);
        }
    }
}
