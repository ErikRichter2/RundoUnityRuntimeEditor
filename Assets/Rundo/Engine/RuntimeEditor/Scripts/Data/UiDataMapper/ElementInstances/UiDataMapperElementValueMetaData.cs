namespace Rundo.RuntimeEditor.Data.UiDataMapper
{
    /// <summary>
    /// Defines a logic properties of a single mapped UI element.
    /// </summary>
    public struct UiDataMapperElementValueMetaData
    {
        /// <summary>
        ///     <para>Index of element if UiDataMapper renders a list of values</para>
        /// </summary>
        public int? ListIndex;
        
        /// <summary>
        ///     <para>Index of mapped data in dataset</para>
        /// </summary>
        public int DataIndex;

        /// <summary>
        ///     <para>Value should not be written into undo/redo system - is true when the element changes values
        ///           by dragging cursor</para>
        /// </summary>
        public readonly bool IgnoreUndoRedo;

        public UiDataMapperElementValueMetaData(bool ignoreUndoRedo, int dataIndex, int? listIndex)
        {
            IgnoreUndoRedo = ignoreUndoRedo;
            DataIndex = dataIndex;
            ListIndex = listIndex;
        }
    }

}
