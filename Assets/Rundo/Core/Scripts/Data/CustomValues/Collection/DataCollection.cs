using System;
using System.Collections;
using System.Collections.Generic;

namespace Rundo.Core.Data
{
    public class DataCollection<TCollection, TData> : IDataCollection<TData>, IParentable, IList
        where TCollection: List<TData>, new()
    {
        private TCollection _collection = new TCollection();

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_collection).CopyTo(array, index);
        }

        [NonSerialized]
        private object _syncRoot;

        public int Count => _collection.Count;
        public bool IsSynchronized => false;
        public object SyncRoot => _syncRoot;
        public bool IsReadOnly => (_collection as IList<TData>).IsReadOnly;

        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (TData)value;
        }

        public IParentable Parent { get; private set; }
        
        protected DataCollection() {}

        public void Add(TData item)
        {
            _collection.Add(item);
            if (item is IParentable parentable)
                parentable.SetParent(this);
        }

        public void AddRange(IEnumerable<TData> items)
        {
            foreach (var it in items)
            {
                if (it is IParentable parentable)
                    parentable.SetParent(this);
                _collection.Add(it);
            }
        }

        int IList.Add(object value)
        {
            Add((TData)value);
            return Count - 1;
        }

        public void Clear()
        {
            foreach (var it in _collection)
                if (it is IParentable parentable)
                    parentable.SetParent(null);
            _collection.Clear();
        }

        bool IList.Contains(object value)
        {
            return Contains((TData)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((TData)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (TData)value);
        }

        void IList.Remove(object value)
        {
            Remove((TData)value);
        }

        public bool Contains(TData item)
        {
            return _collection.Contains(item);
        }

        public void CopyTo(TData[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        public bool Remove(TData item)
        {
            if (item is IParentable parentable)
                parentable.SetParent(null);
            return _collection.Remove(item);
        }

        public int IndexOf(TData item)
        {
            return _collection.IndexOf(item);
        }

        public void Insert(int index, TData item)
        {
            if (item is IParentable parentable)
                parentable.SetParent(this);
            _collection.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            var item = this[index];
            if (item is IParentable parentable)
                parentable.SetParent(null);
            _collection.RemoveAt(index);
        }

        bool IList.IsFixedSize => false;

        public TData this[int index]
        {
            get => _collection[index];
            set
            {
                if (value is IParentable parentable)
                    parentable.SetParent(this);
                _collection[index] = value;
            }
        }

        public IEnumerator<TData> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOfDynamic(object obj)
        {
            if (obj is TData t)
                return IndexOf(t);
            throw new Exception(
                $"Cannot cast dynamic object type {obj.GetType().Name} to collection type {typeof(TData).Name}");
        }

        public void InsertDynamic(int insertAt, object obj)
        {
            if (obj is TData t)
            {
                Insert(insertAt, t);
                return;
            }

            throw new Exception(
                $"Cannot cast dynamic object type {obj.GetType().Name} to collection type {typeof(TData).Name}");
        }

        public void RemoveDynamic(object obj)
        {
            if (obj is TData t)
            {
                Remove(t);
                return;
            }

            throw new Exception(
                $"Cannot cast dynamic object type {obj.GetType().Name} to collection type {typeof(TData).Name}");
        }

        public object[] GetValues()
        {
            if (_collection == null)
                return null;

            var arr = new object[_collection.Count];
            for (int i = 0; i < _collection.Count; ++i)
                arr[i] = _collection[i];

            return arr;
        }

        public void SetParent(IParentable parent)
        {
            Parent = parent;
        }

        public T GetParentInHierarchy<T>()
        {
            if (this is T t)
                return t;
            if (Parent is IParentable parentable)
                return parentable.GetParentInHierarchy<T>();
            return default;
        }
    }
}

