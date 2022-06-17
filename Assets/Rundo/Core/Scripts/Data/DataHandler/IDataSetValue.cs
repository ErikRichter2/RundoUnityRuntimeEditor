namespace Rundo.Core.Data
{
    public interface IDataSetValue
    {
        object ValueDynamic { get; }
        int DataIndex { get; }
        int? ListIndex { get; }
        bool IgnoreUndoRedo { get; }
    }
}
