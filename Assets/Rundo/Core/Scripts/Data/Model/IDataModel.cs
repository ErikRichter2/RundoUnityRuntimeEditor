using System;
using System.Collections.Generic;

namespace Rundo.Core.Data
{
    public interface IDataModel<out T>
    {
        void Modify(string data, bool ignoreUndoRedo = false);
        
        void Modify(Action<T> onModify, bool ignoreUndoRedo = false);

        void ClearCollection<TChild>(Func<T, IList<TChild>> collectionGetter,
            bool ignoreUndoRedo = false);

        void AddToCollection<TChild>(Func<T, IList<TChild>> collectionGetter, TChild child,
            int collectionIndex, bool ignoreUndoRedo = false);

        void AddToCollection<TChild>(Func<T, IList<TChild>> collectionGetter, TChild child,
            bool ignoreUndoRedo = false);

        void RemoveFromCollectionAt<TChild>(Func<T, IList<TChild>> collectionGetter,
            int removeAt, bool ignoreUndoRedo = false);

        void RemoveFromCollection<TChild>(Func<T, IList<TChild>> collectionGetter,
            TChild child, bool ignoreUndoRedo = false);
        
        T Data { get; }

        T Copy();
    }

}

