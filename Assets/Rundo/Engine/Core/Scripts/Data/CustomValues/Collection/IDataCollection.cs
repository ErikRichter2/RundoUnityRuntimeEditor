using System.Collections.Generic;

namespace Rundo.Core.Data
{
    public interface IDataCollection
    {
        int IndexOfDynamic(object obj);
        void InsertDynamic(int insertAt, object obj);
        void RemoveDynamic(object obj);
        object[] GetValues();
    }
    
    public interface IDataCollection<T> : IDataCollection, IList<T>
    {
    }
}

