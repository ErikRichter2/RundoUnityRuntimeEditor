namespace Rundo.RuntimeEditor.Data.UiDataMapper
{
    public struct UiDataMapperElementDataMetaData
    {
        /// <summary>
        ///     <para>Index of element if UiDataMapper renders a list of values</para>
        /// </summary>
        public int? ListIndex;
        
        /// <summary>
        ///     <para>Index of mapped data in dataset</para>
        /// </summary>
        public int DataIndex;

        public UiDataMapperElementDataMetaData(int dataIndex, int? listIndex)
        {
            DataIndex = dataIndex;
            ListIndex = listIndex;
        }
    }
    
}
