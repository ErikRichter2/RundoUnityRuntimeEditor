using System;
using System.Collections.Generic;
using Rundo.Core.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rundo.RuntimeEditor.Behaviours
{
    public interface ISimpleScrollViewControllerData
    {
        GameObject InstantiateAtIndex(int index, Transform parent = null);
        void ActivatedAtIndex(int index);
        void DeactivatedAtIndex(int index);
        bool IsSizeProvider { get; }
        Vector2 GetSizeAtIndex(int index);
        RectTransform GetRectTransformAtIndex(int index);
        int Count { get; }
        bool IsInvisibleAtIndex(int index);
        bool IsInstantiatedAtIndex(int index);
        void DestroyAtIndex(int index);
    }

    public class SimpleScrollViewControllerData<T> : ISimpleScrollViewControllerData
    {
        public class ScrollItemData
        {
            public T Data;
            public int Index;
            public bool IsInvisible;
            public GameObject GameObject;
            public Vector2 SizeUI;
            public bool SizeProvided;
            public bool IsInstantiated;
            public RectTransform RectTransform;
        }

        public bool IsSizeProvider => SizeProvider != null;

        public Func<T, Transform, int, GameObject> InstanceProvider;
        public Func<T, Vector2Int> SizeProvider;
        public Action<int, GameObject> DataActivated;
        public Action<int, GameObject> DataDeactivated;
        
        private readonly List<ScrollItemData> Data = new List<ScrollItemData>();

        private int _prevSelectedIndex;
        public List<int> SelectedIndexes { get; } = new List<int>();

        private void AddSelectionToList(List<T> selection, T item, bool isCtrl)
        {
            if (isCtrl)
            {
                if (selection.Contains(item))
                    selection.Remove(item);
                else
                    selection.Add(item);
            }
            else
            {
                if (selection.Count > 1)
                {
                    selection.Clear();
                    selection.Add(item);
                }
                else
                {
                    var isSelected = selection.Contains(item);
                    selection.Clear();
                    if (isSelected)
                        selection.Remove(item);
                    else
                        selection.Add(item);
                }
            }
        }


        public GameObject InstantiateAtIndex(int index, Transform parent = null)
        {
            var data = Data[index];
            var obj = InstanceProvider(data.Data, parent, index);
            data.GameObject = obj;
            data.RectTransform = obj.GetComponent<RectTransform>();
            data.IsInstantiated = true;
            return obj;
        }

        public bool IsInstantiatedAtIndex(int index)
        {
            return Data[index].IsInstantiated;
        }

        public void DestroyAtIndex(int index)
        {
            var data = Data[index];
            if (data.IsInstantiated)
            {
                Object.Destroy(data.GameObject);
                data.IsInstantiated = false;
                data.RectTransform = null;
                data.GameObject = null;
            }
        }

        public void ActivatedAtIndex(int index)
        {
            DataActivated?.Invoke(index, Data[index].GameObject);
        }

        public void DeactivatedAtIndex(int index)
        {
            DataDeactivated?.Invoke(index, Data[index].GameObject);
        }

        public Vector2 GetSizeAtIndex(int index)
        {
            if (Data[index].SizeProvided)
                return Data[index].SizeUI;
            Data[index].SizeProvided = true;
            Data[index].SizeUI = SizeProvider?.Invoke(Data[index].Data) ?? Vector2.zero;
            return Data[index].SizeUI;
        }

        public RectTransform GetRectTransformAtIndex(int index)
        {
            return Data[index].RectTransform;
        }

        public U GetComponentAtIndex<U>(int index) where U: Component
        {
            return Data[index].GameObject?.GetComponent<U>();
        }

        public bool IsInvisibleAtIndex(int index)
        {
            return Data[index].IsInvisible;
        }

        public void SetIsInvisibleAtIndex(int index, bool value, bool withoutNotify = false)
        {
            Data[index].IsInvisible = value;
        }

        public void Add(T item, bool withoutNotify = false)
        {
            Data.Add(new ScrollItemData
            {
                Data = item,
                Index = Data.Count,
            });
        }

        public void Add(IEnumerable<T> items)
        {
            foreach (var it in items)
            {
                Data.Add(new ScrollItemData
                {
                    Data = it,
                    Index = Data.Count,
                });
            }
        }

        public T Get(int index)
        {
            return Data[index].Data;
        }

        public void Clear()
        {
            Data.Clear();
        }

        public int GetIndex(T data)
        {
            foreach (var it in Data)
                if (ReferenceEquals(it.Data, data))
                    return it.Index;
            return -1;
        }

        public void SortData(Func<T, T, int> sortFunction)
        {
            Data.Sort((item1, item2) => sortFunction(item1.Data, item2.Data));
        }

        public IEnumerable<ScrollItemData> GetActive()
        {
            var res = new List<ScrollItemData>();
            foreach (var it in Data)
                if (it.IsInstantiated && it.IsInvisible == false)
                    res.Add(it);
            return res;
        }

        public int Count => Data.Count;
    }
}

