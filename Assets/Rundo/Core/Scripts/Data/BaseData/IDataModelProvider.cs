namespace Rundo.Core.Data
{
    public interface IDataModelProvider
    {
        IDataModel<T> GetModel<T>();
    }
}

