using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rundo.Core.Commands;
using Rundo.Core.Data;
using Rundo.Core.Events;
using Rundo.RuntimeEditor.Behaviours;
using Rundo.Core.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rundo.RuntimeEditor.Data
{
    public struct DataSceneMetaData : IPersistentDataSetMetaData
    {
        [JsonIgnore]
        public string PersistentMetaDataGuid => Guid.ToStringRawValue();

        public string Name;
        public TGuid<DataScene.TDataSceneId> Guid;
    }
    
    public class DataScene : BaseData, ICommandProcessorProvider, IDataGameObjectFinder, IDataGameObjectContainer
    {
        public struct TDataSceneId {}
        
        public DataList<DataGameObject> DataGameObjects;
        public DataSceneMetaData DataSceneMetaData;

        [JsonIgnore]
        public ICommandProcessor CommandProcessor { get; private set; } = new CommandProcessor();
        
        protected DataScene() {}

        public override void OnInstantiated()
        {
            base.OnInstantiated();
            DataGameObjects = InstantiateList<DataGameObject>();
            DataSceneMetaData.Guid = TGuid<TDataSceneId>.Create();
            DataSceneMetaData.Name = "Scene";
        }

        public void SetCommandProcessor(ICommandProcessor commandProcessor)
        {
            CommandProcessor = commandProcessor;
        }

        public void CheckObjectIdsBeforeAdd(IParentable parent, DataGameObject child)
        {
            if (parent.GetParentInHierarchy<DataScene>() == this)
            {
                var gameObjectsIds = new List<DataGameObjectId>();
                foreach (var it in GetAllSceneDataGameObjects())
                    gameObjectsIds.Add(it.ObjectId);
            
                var queue = new Queue<DataGameObject>();
                queue.Enqueue(child);

                while (queue.Count > 0)
                {
                    var go = queue.Dequeue();
                    if (gameObjectsIds.Contains(go.ObjectId))
                        throw new Exception(
                            $"Cannot add gameObject to scene with id {go.ObjectId.ToStringRawValue()}, id already exists");
                    
                    QueueUtils.EnqueueList(queue, go.Children);
                }
            }
        }

        public DataList<DataGameObject> GetCollection()
        {
            return DataGameObjects;
        }

        public DataGameObject Find(DataGameObjectId dataGameObjectId)
        {
            return DataGameObject.FindDataGameObject(this, dataGameObjectId);
        }
        
        public DataGameObject[] GetTopLevelDataGameObjects()
        {
            return DataGameObjects.ToArray();
        }
        
        public DataGameObject[] GetAllSceneDataGameObjects()
        {
            var queue = new Queue<DataGameObject>();
            var res = new List<DataGameObject>();
            
            foreach (var it in GetTopLevelDataGameObjects())
                queue.Enqueue(it);

            while (queue.Count > 0)
            {
                var it = queue.Dequeue();
                res.Add(it);
                
                foreach (var child in it.Children)
                    queue.Enqueue(child);
            }

            return res.ToArray();
        }
        
        public DataGameObject InstantiateDataGameObjectFromPrefab(PrefabIdBehaviour prefab)
        {
            var prefabInstance = Object.Instantiate(prefab);
            var dataGameObject = DataGameObject.Instantiate();

            dataGameObject.PrefabId = prefab.Guid;

            InstantiateDataGameObjectFromGameObject(dataGameObject, prefabInstance.gameObject);
            Object.Destroy(prefabInstance.gameObject);
            
            return dataGameObject;
        }

        private void InstantiateDataGameObjectFromGameObject(DataGameObject dataGameObject, GameObject gameObject)
        {
            foreach (var it in gameObject.GetComponents<Component>())
            {
                if (RundoEngine.ReflectionService.IsAllowedComponent(it.GetType()))
                {
                    var dataComponent = dataGameObject.AddComponent(it.GetType());
                    dataComponent.CopyFrom(it);
                    dataComponent.DataComponentPrefab = new DataComponentPrefab();
                }
            }

            var dataGameObjectBehaviour = dataGameObject.GetComponent<DataGameObjectBehaviour>();
            dataGameObjectBehaviour ??= dataGameObject.AddComponent<DataGameObjectBehaviour>();
            dataGameObjectBehaviour.DataComponentPrefab = new DataComponentPrefab();
            dataGameObjectBehaviour.Data.Name = gameObject.name;
            dataGameObjectBehaviour.Data.IsActive = gameObject.activeSelf;

            var dataTransform = dataGameObject.GetComponent<DataTransformBehaviour>();
            if (dataTransform != null)
            {
                dataTransform.DataComponentPrefab = new DataComponentPrefab();
                dataTransform.Data.LocalEulerAngles = gameObject.transform.localEulerAngles;
                dataTransform.Data.LocalScale = gameObject.transform.localScale;
                dataTransform.Data.LocalPosition = gameObject.transform.localPosition;
            }

            foreach (Transform child in gameObject.transform)
            {
                var dataChild = DataGameObject.Instantiate();
                dataChild.SetDataGameObjectParent(dataGameObject);
                InstantiateDataGameObjectFromGameObject(dataChild, child.gameObject);
            }

            dataGameObject.IsFromPrefab = true;
        }

        public async Task<GameObject> InstantiateGameObject(
            IBaseDataProviderBehaviour dataProviderBehaviour,
            DataGameObject dataGameObject,
            Transform parent,
            bool initial)
        {
            if (initial && dataGameObject.PrefabId.IsNullOrEmpty == false)
            {
                /*
                void ProcessDataGameObject(DataGameObject lPrefabData, DataGameObject lInstancedData)
                {
                    lPrefabData.ObjectId = lInstancedData.ObjectId;
                    
                    var processedComponents = new List<DataComponent>();
                    
                    foreach (var prefabComponent in lPrefabData.Components)
                    {
                        foreach (var dataComponent in lInstancedData.Components)
                        {
                            if (processedComponents.Contains(prefabComponent) == false &&
                                dataComponent.GetComponentType() == prefabComponent.GetComponentType() &&
                                dataComponent.IsOverriden())
                            {
                                processedComponents.Add(prefabComponent);
                                RundoEngine.DataSerializer.PopulateObject(dataComponent, prefabComponent);
                            }
                        }
                    }

                    var childCount = Math.Min(lPrefabData.Children.Count, lInstancedData.Children.Count);
                    for (var i = 0; i < childCount; ++i)
                        ProcessDataGameObject(lPrefabData.Children[i], lInstancedData.Children[i]);
                }
*/
                var prefabIdBehaviour = await dataProviderBehaviour.LoadPrefab(dataGameObject.PrefabId);

                if (prefabIdBehaviour != null)
                {
                    var dataFromPrefab =
                        InstantiateDataGameObjectFromPrefab(prefabIdBehaviour);
                    
                    // synchronize children

                    var queue = new Queue<(DataGameObject, DataGameObject)>();
                    queue.Enqueue((dataGameObject, dataFromPrefab));

                    while (queue.Count > 0)
                    {
                        var it = queue.Dequeue();
                        var instance = it.Item1;
                        var prefab = it.Item2;

                        // synchronize components prefab -> instance
                        // keep overrided components, update/remove not-overrided components
                        SynchronizeComponents(instance, prefab);

                        // add
                        for (var i = instance.Children.Count; i < prefab.Children.Count; ++i)
                            instance.Children.Add(RundoEngine.DataSerializer.Copy(prefab.Children[i]));
                    
                        // remove
                        for (var i = prefab.Children.Count; i < instance.Children.Count; ++i)
                            instance.Children.RemoveAt(i);

                        for (var i = 0; i < instance.Children.Count; ++i)
                            queue.Enqueue((instance.Children[i], prefab.Children[i]));
                    }
                }
            }

            return await InstantiateGameObjectFromDataGameObject(dataProviderBehaviour, dataGameObject, parent);
        }

        private void SynchronizeComponents(DataGameObject instance, DataGameObject prefab)
        {
            var processedComponents = new List<DataComponent>(prefab.Components);
            
            foreach (var component in instance.Components.ToArray())
            {
                if (component.DataComponentPrefab == null)
                    continue;

                var found = false;
                
                // find same component type in the prefab
                foreach (var prefabComponent in processedComponents)
                {
                    if (prefabComponent.GetComponentType() != component.GetComponentType())
                        continue;

                    found = true;

                    // copy values from prefab component to the instance component
                    processedComponents.Remove(prefabComponent);

                    if (component.IsOverriden() == false)
                        RundoEngine.DataSerializer.PopulateObject(prefabComponent, component);
                    
                    break;
                }

                // mark component as removed
                if (found == false)
                    instance.Components.Remove(component);
            }
            
            // add components from prefab
            foreach (var prefabComponent in processedComponents)
                instance.Components.Add(prefabComponent);
        }

        private async Task<GameObject> InstantiateGameObjectFromDataGameObject(
            IBaseDataProviderBehaviour dataProviderBehaviour,
            DataGameObject dataGameObject, 
            Transform parent,
            GameObject gameObjectInstance = null)
        {   
            GameObject go = null;
            var isFromPrefab = false;

            if (gameObjectInstance != null)
            {
                isFromPrefab = true;
                go = gameObjectInstance;
            }
            else if (dataGameObject.PrefabId.IsNullOrEmpty == false)
            {
                var prefab = await dataProviderBehaviour.LoadPrefab(dataGameObject.PrefabId);
                if (prefab != null)
                {
                    isFromPrefab = true;
                    go = Object.Instantiate(prefab).gameObject;
                }
            }

            go ??= new GameObject();
            
            go.transform.SetParent(parent, true);

            if (go.TryGetComponent<DataGameObjectBehaviour>(out var dataGameObjectBehaviour) == false)
            {
                dataGameObjectBehaviour = go.AddComponent<DataGameObjectBehaviour>();
                dataGameObjectBehaviour.Name = go.name;
                dataGameObjectBehaviour.IsActive = go.activeSelf;
            }
            
            if (go.TryGetComponent<GameObjectComponentsRebuilderBehaviour>(out var gameObjectComponentsRebuilderBehaviour) == false)
                gameObjectComponentsRebuilderBehaviour = go.AddComponent<GameObjectComponentsRebuilderBehaviour>();

            dataProviderBehaviour.PostprocessGameObject(go);
            
            gameObjectComponentsRebuilderBehaviour.SetDataGameObject(dataGameObject);

            for (var i = 0; i < dataGameObject.Children.Count; ++i)
            {
                GameObject childGo = null;
                if (isFromPrefab)
                    if (go.transform.childCount > i)
                        childGo = go.transform.GetChild(i).gameObject;
                await InstantiateGameObjectFromDataGameObject(dataProviderBehaviour, dataGameObject.Children[i], go.transform, childGo);
            }

            return go;
        }

        public List<DataGameObjectTreeHierarchy> GetTreeHierarchy()
        {
            var list = new List<DataGameObjectTreeHierarchy>();
            foreach (var dataGameObject in GetTopLevelDataGameObjects())
                list.Add(new DataGameObjectTreeHierarchy
                {
                    Depth = 0,
                    DataGameObject = dataGameObject
                });

            var res = new List<DataGameObjectTreeHierarchy>();
            while (list.Count > 0)
            {
                var metadata = list[0];
                list.RemoveAt(0);
                
                res.Add(metadata);

                var isExpanded =
                    RuntimeEditorBehaviour.PersistentEditorPrefs.LoadData().ExpandedDataGameObjectsInHierarchyWindow.Contains(
                        metadata.DataGameObject.ObjectId);
                
                for (var i = 0; i < metadata.DataGameObject.Children.Count; ++i)
                    list.Insert(i, new DataGameObjectTreeHierarchy
                    {
                        IsHidden = isExpanded == false,
                        Depth = metadata.Depth + 1,
                        DataGameObject = metadata.DataGameObject.Children[i]
                    });
            }

            return res;
        }

    }
}

