using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Behaviours;
using Rundo.RuntimeEditor.Commands;

namespace Rundo.RuntimeEditor.Data
{
    public sealed class DataGameObject : BaseData, IDataGameObjectContainer, IDataSerializerPostProcessHandler
    {
        [GenerateNewGuidWhenClone]
        public DataGameObjectId ObjectId;

        [JsonIgnore]
        public bool IsDestroyed;

        [JsonIgnore]
        public bool IsRemovedFromPrefab;
        
        [JsonIgnore]
        public string Name
        {
            get => GetComponent<DataGameObjectBehaviour>().Data.Name;
            set => GetComponent<DataGameObjectBehaviour>().Data.Name = value;
        }

        [JsonIgnore]
        public bool IsActive
        {
            get => GetComponent<DataGameObjectBehaviour>().Data.IsActive;
            set => GetComponent<DataGameObjectBehaviour>().Data.IsActive = value;
        }

        public TGuid<TPrefabId> PrefabId;
        public bool IsFromPrefab;

        public DataList<DataGameObject> Children;
        public DataList<DataComponent> Components;

        [JsonIgnore] public DataScene DataScene => GetParentInHierarchy<DataScene>();
            
        public override void OnInstantiated()
        {
            base.OnInstantiated();
            ObjectId.CreateNewGUID();
            Children = InstantiateList<DataGameObject>();
            Components = InstantiateList<DataComponent>();
        }

        public static DataGameObject Instantiate()
        {
            var res = RundoEngine.DataFactory.Instantiate<DataGameObject>();
            res.AddComponent<DataGameObjectBehaviour>();
            res.AddComponent<DataTransformBehaviour>();
            return res;
        }
        
        public static void Destroy(DataGameObject dataGameObject)
        {
            if (dataGameObject == null)
                return;
            
            DestroyDataGameObjectCommand.Process(dataGameObject.DataScene, dataGameObject);
        }

        private List<DataGameObjectId> GetDataGameObjectIds()
        {
            var res = new List<DataGameObjectId>();
            
            var queue = new Queue<DataGameObject>();
            queue.Enqueue(this);

            while (queue.Count > 0)
            {
                var go = queue.Dequeue();
                res.Add(go.ObjectId);
                foreach (var child in go.Children)
                    queue.Enqueue(child);
            }
            
            return res;
        }

        public IDataGameObjectContainer GetDataGameObjectParent()
        {
            if (Parent == null)
                return null;
            return Parent.GetParentInHierarchy<IDataGameObjectContainer>();
        }

        public void SetDataGameObjectParent(IDataGameObjectContainer dataGameObjectContainer, int childIndex = -1)
        {
            SetDataGameObjectParentCommand.Process(dataGameObjectContainer, this, childIndex);
        }
        
        public DataComponent<T> AddComponent<T>()
        {
            return (DataComponent<T>)AddComponent(typeof(T));
        }

        public DataComponent AddComponent(Type type, bool ignoreUndoRedo = false)
        {
            return AddComponent(DataComponent.Instantiate(type, this), ignoreUndoRedo);
        }

        public DataComponent AddComponent(DataComponent dataComponent, bool ignoreUndoRedo = false)
        {
            dataComponent.SetParent(this);
            dataComponent.SetDataGameObjectParent(this);
            GetModel<DataGameObject>()
                .AddToCollection(collectionGetter => collectionGetter.Components, dataComponent, ignoreUndoRedo);
            return dataComponent;
        }

        public bool HasComponent(DataComponent dataComponent)
        {
            foreach (var it in Components)
                if (dataComponent == it)
                    return true;
            return false;
        }

        public IDataComponent<T> GetComponent<T>()
        {
            foreach (var it in Components)
                if (it is IDataComponent<T> t)
                    return t;

            return default;
        }

        public List<IDataComponent<T>> GetComponents<T>()
        {
            var res = new List<IDataComponent<T>>();
            
            foreach (var it in Components)
                if (typeof(T).IsAssignableFrom(it.GetComponentType()))
                    res.Add(it as IDataComponent<T>);

            return res;
        }
        
        public DataComponent<T> GetComponentInParent<T>()
        {
            foreach (var it in Components)
                if (it is DataComponent<T> t)
                    return t;

            var parent = GetDataGameObjectParent();

            if (parent is DataGameObject dataGameObject)
            {
                var res = dataGameObject.GetComponentInParent<T>();
                if (res != null)
                    return res;
            }

            return default;
        }
        

        public DataComponent<T> GetComponentInChildren<T>()
        {
            foreach (var it in Components)
                if (it is DataComponent<T> t)
                    return t;

            foreach (var it in Children)
            {
                var found = it.GetComponentInChildren<T>();
                if (found != null)
                    return found;
            }

            return default;
        }

        public void RemoveComponent(DataComponent dataComponent)
        {
            if (dataComponent.DataComponentPrefab != null)
                throw new Exception($"Cant remove component {dataComponent.GetType().Name} created from prefab.");
            
            GetModel<DataGameObject>().RemoveFromCollection(collection => collection.Components, dataComponent);
        }

        public bool HasComponent<T>()
        {
            foreach (var it in Components)
            {
                if (it is DataComponent<T> _)
                    return true;
                if (it.GetComponentType() == typeof(DataComponent<T>))
                    return true;
            }

            return false;
        }

        public bool HasComponentOfType(Type type)
        {
            foreach (var it in Components)
                if (it.GetComponentType() == type)
                    return true;

            return false;
        }

        public List<DataComponent> GetComponentsOfType(Type type)
        {
            var res = new List<DataComponent>();
            foreach (var it in Components)
                if (it.GetComponentType() == type)
                    res.Add(it);
            return res;
        }

        public DataList<DataGameObject> GetCollection()
        {
            return Children;
        }

        public DataGameObject Find(DataGameObjectId dataGameObjectId)
        {
            return FindDataGameObject(this, dataGameObjectId);
        }

        public static DataGameObject FindDataGameObject(IDataGameObjectContainer dataGameObjectContainer, DataGameObjectId dataGameObjectId)
        {
            foreach (var it in dataGameObjectContainer.GetCollection())
                if (it.ObjectId == dataGameObjectId)
                    return it;
            foreach (var it in dataGameObjectContainer.GetCollection())
            {
                var found = it.Find(dataGameObjectId);
                if (found != null)
                    return found;
            }
            return null;
        }

        public void PostProcessDataSerialization()
        {
            var components = new List<DataComponent>(Components);
            Components.Clear();

            foreach (var component in components)
            {
                if (component != null)
                {
                    Components.Add(component);
                    component.SetDataGameObjectParent(this);
                }
            }
        }
    }
}

