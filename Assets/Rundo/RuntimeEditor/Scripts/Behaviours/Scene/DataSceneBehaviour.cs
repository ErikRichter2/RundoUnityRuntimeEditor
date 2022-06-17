using System.Collections.Generic;
using Rundo.RuntimeEditor.Commands;
using Rundo.RuntimeEditor.Data;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// Instantiates a game objects in sync with a data model - basically represents a Unity-scene.
    /// </summary>
    public class DataSceneBehaviour : BaseBehaviour, IDataGameObjectBehaviourFinder
    {
        private readonly List<DataGameObjectBehaviour> _instantiatedDataGameObjects = new List<DataGameObjectBehaviour>();
    
        private void Start()
        {
            RegisterCommandListener<CreateDataGameObjectCommand>(OnCreateDataGameObjectCommand);
            RegisterCommandListener<DestroyDataGameObjectCommand>(OnDestroyDataGameObjectCommand);
            RegisterCommandListener<SetDataGameObjectParentCommand>(OnSetDataGameObjectParent);

            var scene = DataScene;

            if (scene != null)
                foreach (var dataGameObject in scene.DataGameObjects)
                    CreateEditorObject(dataGameObject, scene);
        }

        public void SetData()
        {
            
        }

        private void OnCreateDataGameObjectCommand(CreateDataGameObjectCommand data)
        {
            if (data.Data == DataScene)
                CreateEditorObject(data.DataGameObject, data.Parent);
        }
    
        private void OnDestroyDataGameObjectCommand(DestroyDataGameObjectCommand data)
        {
            if (data.Data == DataScene)
                DestroyEditorObject(data.DataGameObject);
        }
        
        private async void CreateEditorObject(DataGameObject dataGameObject, IDataGameObjectContainer parent)
        {
            var transformParent = transform;
            var parentGo = Find(parent);
            if (parentGo != null)
                transformParent = parentGo.transform;
            
            var go = await DataScene.InstantiateGameObject(BaseDataProvider, dataGameObject, transformParent);

            var queue = new Queue<GameObject>();
            queue.Enqueue(go);

            while (queue.Count > 0)
            {
                var it = queue.Dequeue();
                
                if (it.TryGetComponent<DataGameObjectBehaviour>(out var dataGameObjectBehaviour))
                    _instantiatedDataGameObjects.Add(dataGameObjectBehaviour);
                
                foreach (Transform child in it.transform)
                    queue.Enqueue(child.gameObject);
            }
        }

        private void DestroyEditorObject(DataGameObject dataGameObject)
        {
            foreach (var worldObj in _instantiatedDataGameObjects)
                if (worldObj.DataGameObject == dataGameObject)
                {
                    _instantiatedDataGameObjects.Remove(worldObj);
                    Destroy(worldObj.gameObject);
                    return;
                }
        }

        public DataGameObjectBehaviour Find(IDataGameObjectContainer dataGameObjectContainer)
        {
            if (dataGameObjectContainer is DataScene dataScene)
                return null;
            foreach (var it in _instantiatedDataGameObjects)
                if (it.DataGameObject == dataGameObjectContainer)
                    return it.GetComponent<DataGameObjectBehaviour>();
            return null;
        }

        public DataGameObjectBehaviour Find(DataGameObjectId dataGameObjectId)
        {
            foreach (var it in _instantiatedDataGameObjects)
                if (it.DataGameObject.ObjectId == dataGameObjectId)
                    return it.GetComponent<DataGameObjectBehaviour>();
            return null;
        }

        private void OnSetDataGameObjectParent(SetDataGameObjectParentCommand command)
        {
            var parentNewGo = Find(command.ParentNew);
            var childGo = Find(command.Child);
            if (parentNewGo != null)
                childGo.transform.SetParent(parentNewGo.transform, true);
            else
                childGo.transform.SetParent(transform, true);
            childGo.transform.SetSiblingIndex(command.ChildIndexNew);
        }
    }
}



