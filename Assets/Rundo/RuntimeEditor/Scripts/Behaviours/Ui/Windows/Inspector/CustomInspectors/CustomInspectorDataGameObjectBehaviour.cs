using System;
using System.Collections.Generic;
using Rundo.Core.Commands;
using Rundo.Core.Data;
using Rundo.Core.Utils;
using Rundo.RuntimeEditor.Attributes;
using Rundo.RuntimeEditor.Data;
using Rundo.RuntimeEditor.Tools;
using Rundo.Ui;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    [CustomInspector(typeof(DataGameObject), false)]
    public class CustomInspectorDataGameObjectBehaviour : InspectorBaseBehaviour
    {
        [SerializeField] private GameObject _componentsContent;
        [SerializeField] private DropDownBehaviour _addComponent;

        protected override void MapUi()
        {
            RegisterCommandListener<ICollectionModifierParent<DataGameObject>>(Rebuild);

            InitComponentsDropDown();

            _addComponent.OnSubmit(value =>
            {
                var type = Type.GetType(value.Value);
                if (type != null)
                {
                    foreach (var it in UiDataMapper.DataHandler.GetRootData().ToArray())
                        ((DataGameObject)it).AddComponent(type);
                }
            });
        }

        protected override void OnDataSetInternal()
        {
            base.OnDataSetInternal();
            Rebuild();
        }

        private void InitComponentsDropDown()
        {
            _addComponent.GetDataProvider().Clear();
            foreach (var it in RundoEngine.ReflectionService.GetAllowedComponents())
            {
                _addComponent.AddData(it.AssemblyQualifiedName, StringUtils.ToMaxLength(StringUtils.ToPascalCase(it.Name), 35));
            }
        }

        private void Rebuild()
        {
            foreach (Transform child in _componentsContent.transform)
                Destroy(child.gameObject);
            
            var excludedComponents = new List<Type>();
            var componentsMaxCount = new Dictionary<Type, int>();
            var allComponents = new List<Type>();
            var dataGameObjects = UiDataMapper.DataHandler.GetRootDataTyped<DataGameObject>();
            
            // get components that are in each data
            foreach (var dataGameObject1 in dataGameObjects)
            {
                var componentTypeCount = new Dictionary<Type, int>();
                
                foreach (var component1 in dataGameObject1.Components)
                {
                    var componentType1 = component1.GetComponentType();
                    
                    if (allComponents.Contains(componentType1) == false)
                        allComponents.Add(componentType1);

                    if (componentTypeCount.ContainsKey(componentType1) == false)
                        componentTypeCount[componentType1] = 0;
                    componentTypeCount[componentType1]++;

                    if (excludedComponents.Contains(componentType1))
                        continue;
                    
                    foreach (var dataGameObject2 in dataGameObjects)
                    {
                        if (dataGameObject2.HasComponentOfType(componentType1) == false)
                        {
                            excludedComponents.Add(componentType1);
                            break;
                        }
                    }
                }

                foreach (var it in componentTypeCount)
                {
                    if (componentsMaxCount.ContainsKey(it.Key) == false)
                    {
                        componentsMaxCount[it.Key] = it.Value;
                    }
                    else if (componentsMaxCount[it.Key] < it.Value)
                    {
                        componentsMaxCount[it.Key] = it.Value;
                    }
                }
            }

            var finalComponentTypes = new List<Type>();

            foreach (var it in allComponents)
            {
                if (excludedComponents.Contains(it))
                    continue;
                
                if (dataGameObjects.Count > 1 && componentsMaxCount[it] > 1)
                    continue;
                
                finalComponentTypes.Add(it);
            }

            foreach (var finalType in finalComponentTypes)
            {
                for (var i = 0; i < componentsMaxCount[finalType]; ++i)
                {
                    var data = new List<object>();
                    foreach (var dataGameObject in dataGameObjects)
                        data.Add(dataGameObject.GetComponentsOfType(finalType)[i]);

                    var dataHandler = new DataHandler(UiDataMapper.DataHandler.CommandProcessor);
                    dataHandler.SetRootDataList(data);
                    
                    UiElementsFactory.DrawInspector(dataHandler, _componentsContent.transform, false);
                }
            }

            GetComponentInParent<CanvasRebuilderBehaviour>()?.Rebuild();
        }
    }    
}



