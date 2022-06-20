using System;
using System.Collections.Generic;
using System.Linq;
using Rundo.Core.Commands;

namespace Rundo.Core.Data
{
    /**
     * Stores runtime-only values, helpers, commands for serialized data.
     */
    public class DataModel
    {
        protected object _data;
        
        public static DataModel Instantiate(Type type, object serializedData)
        {
            var instance = (DataModel)RundoEngine.DataFactory.Instantiate(type);
            instance._data = serializedData;
            return instance;
        }

        public virtual void OnInstantiated() {}
    }

    /**
     * Typed model which allows for implementing shortcuts to general-purpose commands. So instead of creating
     * command instances i.e. new ModifySerializedDataCommand(), we could just call SerializedDataModel.Modify()
     */
    public class DataModel<T> : DataModel, IDataModel<T>
    {
        /**
         * Reference to data
         */
        public T Data => (T)_data;
        
        public T Copy()
        {
            var serialized = RundoEngine.DataSerializer.SerializeObject(this);
            var copy = RundoEngine.DataSerializer.DeserializeObject(serialized, GetType());
            return (T)copy;
        }

        public void Modify(string data, bool ignoreUndoRedo = false)
        {
            var command = new ModifyDataCommand<T>(Data, data);
            command.IgnoreUndoRedo = ignoreUndoRedo;
            command.Process();
        }

        public void Modify(Action<T> onModify, bool ignoreUndoRedo = false)
        {
            var modify = (T)RundoEngine.DataSerializer.Clone(Data);
            onModify?.Invoke(modify);
            Modify(modify, ignoreUndoRedo);
        }

        private void Modify(T modify, bool ignoreUndoRedo = false)
        {
            var command =
                new ModifyDataCommand<T>(Data, RundoEngine.DataSerializer.SerializeObject(modify));
            command.IgnoreUndoRedo = ignoreUndoRedo;
            command.Process();
        }
        
        public void ClearCollection<TChild>(Func<T, IList<TChild>> collectionGetter, bool ignoreUndoRedo = false)
        {
            foreach (var child in collectionGetter.Invoke(Data).ToArray())
                RemoveFromCollection(collectionGetter, child, ignoreUndoRedo);
        }

        public void AddToCollection<TChild>(Func<T, IList<TChild>> collectionGetter, 
            TChild child, int collectionIndex, bool ignoreUndoRedo = false)
        {
            var command = AddDataToCollectionCommand.Instantiate(Data, collectionGetter, child, collectionIndex);
            command.IgnoreUndoRedo = ignoreUndoRedo;
            command.Process();
        }

        public void AddToCollection<TChild>(Func<T, IList<TChild>> collectionGetter, 
            TChild child, bool ignoreUndoRedo = false)
        {
            var collection = collectionGetter.Invoke(Data);
            AddToCollection(collectionGetter, child, collection.Count, ignoreUndoRedo);
        }
        
        public void RemoveFromCollectionAt<TChild>(Func<T, IList<TChild>> collectionGetter, 
            int removeAt, bool ignoreUndoRedo = false)
        {
            var collection = collectionGetter.Invoke(Data);
            RemoveFromCollection(collectionGetter, collection[removeAt], ignoreUndoRedo);
        }
        
        public void RemoveFromCollection<TChild>(Func<T, IList<TChild>> collectionGetter, 
            TChild child, bool ignoreUndoRedo = false)
        {
            var command = RemoveDataFromCollectionCommand.Instantiate(Data, collectionGetter, child);
            command.IgnoreUndoRedo = ignoreUndoRedo;
            command.Process();
        }
    }

}

