namespace Rundo.Core.Data
{
    public interface IDataReference
    {
        void SetJsonValue(object value);
        object GetJsonValue();
        void SetExistingDataReferenceValueWrapper(DataReferenceValueWrapper value);
        DataReferenceValueWrapper GetDataReferenceValueWrapper();
    }
}

