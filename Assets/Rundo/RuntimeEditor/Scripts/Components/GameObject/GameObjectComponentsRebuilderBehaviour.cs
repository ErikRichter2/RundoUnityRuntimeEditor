using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Rundo.Core.Commands;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data;
using Rundo.Ui;
using Rundo.Core.Utils;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class GameObjectComponentsRebuilderBehaviour : EditorBaseBehaviour
    {
        [JsonIgnore]
        public DataGameObject DataGameObject { get; private set; }
        
        private readonly Dictionary<DataComponent, Component> _unityComponents = new Dictionary<DataComponent, Component>();

        private void Start()
        {
            RegisterCommandListener<DataGameObject>(OnDataGameObject);
            RegisterCommandListener<DataComponent>(FromDataToBehaviour);
            RegisterCommandListener<ICollectionModifierParent<DataGameObject>>(OnComponentsCollection);
            RebuildComponents();
            Redraw();
        }

        public void SetDataGameObject(DataGameObject dataGameObject)
        {
            DataGameObject = dataGameObject;
            RebuildComponents();
            Redraw();
        }

        private void Redraw()
        {
            gameObject.name = DataGameObject.Name;
            gameObject.SetActive(DataGameObject.IsActive);
        }

        private void OnDataGameObject(DataGameObject dataGameObject)
        {
            if (DataGameObject == dataGameObject)
                Redraw();
        }

        private Type GetNativeComponentType(DataComponent dataComponent)
        {
            var dataComponentType = dataComponent.GetComponentType();

            var dataComponentAttr = dataComponentType.GetCustomAttribute<DataComponentAttribute>();
            if (dataComponentAttr is { BehaviourType: { } })
            {
                if (typeof(Component).IsAssignableFrom(dataComponentAttr.BehaviourType))
                    return dataComponentAttr.BehaviourType;

                throw new Exception(
                    $"Type {dataComponentAttr.BehaviourType.Name} declared in DataComponentAttribute for {dataComponent.GetType()} is not assignable to {nameof(Component)} type.");
            }

            if (typeof(Component).IsAssignableFrom(dataComponentType))
                return dataComponentType;
            
            return default;
        }
        
        private void RebuildComponents()
        {
            List<DataComponent> addedComponents = new List<DataComponent>();
            List<DataComponent> removedComponents = new List<DataComponent>();
            
            foreach (var it in DataGameObject.Components)
                if (_unityComponents.ContainsKey(it) == false)
                    addedComponents.Add(it);
            
            foreach (var it in _unityComponents.Keys)
                if (DataGameObject.HasComponent(it) == false)
                    removedComponents.Add(it);
            
            foreach (var it in addedComponents)
            {
                var nativeComponent = GetNativeComponentType(it);
                
                if (nativeComponent != null)
                {
                    Component componentInstance = null;
                    foreach (var c in gameObject.GetComponents(nativeComponent))
                        if (it.GetComponentType() == c.GetType() && _unityComponents.ContainsValue(c) == false)
                        {
                            componentInstance = c;
                            break;
                        }

                    componentInstance ??= gameObject.AddComponent(nativeComponent);

                    if (componentInstance == null)
                    {
                        Debug.LogError($"Cannot add component of type {nativeComponent.Name}");
                        continue;
                    }
                    
                    _unityComponents[it] = componentInstance;
                    if (componentInstance is IParentable parentable)
                        parentable.SetParent(DataGameObject);
                        
                    // set IsRuntimeOnlyComponent flag
                    // set data component data
                    if (componentInstance is DataComponentMonoBehaviour dataComponentMonoBehaviour)
                    {
                        dataComponentMonoBehaviour.DataGameObject = DataGameObject;
                        dataComponentMonoBehaviour.DataComponent = it;
                        dataComponentMonoBehaviour.IsDataOnlyComponent = false;
                    }
                    else
                    {
                        var member = ReflectionUtils.GetMemberInfo(componentInstance.GetType(),
                            nameof(DataComponentMonoBehaviour.IsDataOnlyComponent));
                        if (member != null)
                            ReflectionUtils.SetValue(componentInstance, member, false);

                        member = ReflectionUtils.GetMemberInfo(componentInstance.GetType(),
                            nameof(DataComponentMonoBehaviour.DataComponent));
                        if (member != null)
                            ReflectionUtils.SetValue(componentInstance, member, it);

                        member = ReflectionUtils.GetMemberInfo(componentInstance.GetType(),
                            nameof(DataComponentMonoBehaviour.DataGameObject));
                        if (member != null)
                            ReflectionUtils.SetValue(componentInstance, member, DataGameObject);
                    }

                    if (it.IsOverridable())
                    {
                        var componentCopy = RundoEngine.DataSerializer.Clone(it);
                        FromBehaviourToData(componentInstance, componentCopy);
                        
                        it.DataComponentPrefab.SetPrefabData(RundoEngine.DataSerializer.SerializeObject(componentCopy.GetData()));
                    
                        if (it.DataComponentPrefab.OverridePrefabComponent == false)
                            it.FromPrefabDataToInstanceData();
                    }
                    
                    FromDataToBehaviour(it);
                }
            }

            foreach (var it in removedComponents)
            {
                Destroy(_unityComponents[it]);
                _unityComponents.Remove(it);
            }
        }

        private void OnComponentsCollection(ICollectionModifierParent<DataGameObject> collectionModifier)
        {
            if (collectionModifier.CollectionOwner != DataGameObject)
                return;

            if (collectionModifier.Collection == DataGameObject.Components)
                RebuildComponents();
        }

        private static void FromBehaviourToData(Component component, DataComponent dataComponent)
        {
            RundoEngine.DataSerializer.PopulateObject(component, dataComponent.GetData());

            // invoke OnFromDataToBehaviour if exists
            if (component is DataComponentMonoBehaviour dataComponentMonoBehaviour)
                dataComponentMonoBehaviour.OnFromBehaviourToData(dataComponent);
            else
            {
                var method = ReflectionUtils.GetMethod(component.GetType(),
                    nameof(DataComponentMonoBehaviour.OnFromBehaviourToData));
                method?.Invoke(component, new object[]{dataComponent});
            }
        }

        public void FromRuntimeBehaviourToData(DataComponent dataComponent)
        {
            if (_unityComponents.ContainsKey(dataComponent))
            {
                var componentBehaviour = _unityComponents[dataComponent];
                RundoEngine.DataSerializer.PopulateObject(componentBehaviour, dataComponent.GetData());

                // invoke OnFromDataToBehaviour if exists
                if (componentBehaviour is DataComponentMonoBehaviour dataComponentMonoBehaviour)
                    dataComponentMonoBehaviour.OnFromBehaviourToData(dataComponent);
                else
                {
                    var method = ReflectionUtils.GetMethod(componentBehaviour.GetType(),
                        nameof(DataComponentMonoBehaviour.OnFromBehaviourToData));
                    method?.Invoke(componentBehaviour, new object[]{dataComponent});
                }
                
                DataGameObject.GetCommandProcessor().EventDispatcher.Dispatch(dataComponent, true);
            }
        }
        
        private void FromDataToBehaviour(DataComponent dataComponent)
        {
            if (dataComponent.SkipUpdateOfRuntimeBehaviour)
                return;
            
            if (_unityComponents.ContainsKey(dataComponent))
            {
                var componentBehaviour = _unityComponents[dataComponent];
                RundoEngine.DataSerializer.PopulateObject(dataComponent.GetData(), componentBehaviour);

                // invoke OnFromDataToBehaviour if exists
                if (componentBehaviour is DataComponentMonoBehaviour dataComponentMonoBehaviour)
                    dataComponentMonoBehaviour.OnFromDataToBehaviour(dataComponent);
                else
                {
                    var method = ReflectionUtils.GetMethod(componentBehaviour.GetType(),
                        nameof(DataComponentMonoBehaviour.OnFromDataToBehaviour));
                    method?.Invoke(componentBehaviour, new object[]{dataComponent});
                }
                
            }
        }
    }
}

