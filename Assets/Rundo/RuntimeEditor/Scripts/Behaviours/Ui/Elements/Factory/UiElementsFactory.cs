using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Rundo.Core.Data;
using Rundo.Core.Utils;
using Rundo.RuntimeEditor.Behaviours;
using Rundo.RuntimeEditor.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rundo.Ui
{
    public static class UiElementsFactory
    {
        private static List<IInspectorBehaviour> _customInspectorPrefabs;
        
        private static string _resourcePath = "Rundo/Ui/Inspector";
        private static string _resourcePathPrimitives = $"{_resourcePath}/Primitives";
        private static string _resourcePathCustom = $"{_resourcePath}/CustomInspector";
        private static string _resourcePathDefault = $"{_resourcePath}/DefaultInspector";

        public static IUiDataMapperElementBehaviour InstantiatePrimitive(Type valueType, string label)
        {
            if (valueType == typeof(bool))
                return Toggle(label);
            if (valueType == typeof(int))
                return InputFieldInt(label);
            if (valueType == typeof(float))
                return InputFieldFloat(label);
            if (valueType == typeof(string))
                return InputFieldString(label);
            if (valueType == typeof(Color))
                return ColorPickerUnityColor(label);
            if (valueType == typeof(TColor))
                return ColorPickerTColor(label);

            if (typeof(IDataComponentReference).IsAssignableFrom(valueType))
                return ObjectPicker(label, valueType);

            if (typeof(IGuid).IsAssignableFrom(valueType))
                return InputFieldGuid(label, valueType);
            
            if (valueType.IsEnum || typeof(ITEnum).IsAssignableFrom(valueType))
            {
                var dropDown = DropDown(label);
                dropDown.FillFromEnum(valueType);
                return dropDown;
            }

            return default;
        }

        public static ColorPickerUnityColorBehaviour ColorPickerUnityColor(string label)
        {
            var prefab = Resources.Load<ColorPickerElementBehaviour>($"{_resourcePathPrimitives}/InspectorColorPickerPrefab");
            var instance = Object.Instantiate(prefab);
            var res = instance.gameObject.AddComponent<ColorPickerUnityColorBehaviour>();
            instance.Label = label;
            return res;
        }

        public static ColorPickerTColorBehaviour ColorPickerTColor(string label)
        {
            var prefab = Resources.Load<ColorPickerElementBehaviour>($"{_resourcePathPrimitives}/InspectorColorPickerPrefab");
            var instance = Object.Instantiate(prefab);
            var res = instance.gameObject.AddComponent<ColorPickerTColorBehaviour>();
            instance.Label = label;
            return res;
        }

        public static ObjectPickerBehaviour ObjectPicker(string label, Type valueType)
        {
            var prefab = Resources.Load<ObjectPickerBehaviour>($"{_resourcePathPrimitives}/InspectorObjectPickerPrefab");
            var instance = Object.Instantiate(prefab);
            instance.Label = label;
            instance.SetReferenceType(valueType.GetGenericArguments()[0]);
            return instance;
        }

        public static InputFieldIntBehaviour InputFieldInt(string label)
        {
            var prefab = Resources.Load<InputFieldBehaviour>($"{_resourcePathPrimitives}/InspectorInputFieldPrefab");
            var instance = Object.Instantiate(prefab);
            instance.Label = label;
            return instance.gameObject.AddComponent<InputFieldIntBehaviour>();;
        }

        public static InputFieldFloatBehaviour InputFieldFloat(string label)
        {
            var prefab = Resources.Load<InputFieldBehaviour>($"{_resourcePathPrimitives}/InspectorInputFieldPrefab");
            var instance = Object.Instantiate(prefab);
            instance.Label = label;
            return instance.gameObject.AddComponent<InputFieldFloatBehaviour>();;
        }

        public static InputFieldStringBehaviour InputFieldGuid(string label, Type guidType)
        {
            var prefab = Resources.Load<InputFieldBehaviour>($"{_resourcePathPrimitives}/InspectorInputFieldPrefab");
            var instance = Object.Instantiate(prefab);
            instance.Label = label;
            var res = instance.gameObject.AddComponent<InputFieldStringBehaviour>();;
            res.SetValueConverter((obj) =>
            {
                var guid = (IGuid)Activator.CreateInstance(guidType);
                guid.SetGUID((string)obj);
                return guid;
            }, (obj) =>
            {
                return ((IGuid)obj).ToStringRawValue();
            });

            return res;
        }

        public static InputFieldStringBehaviour InputFieldString(string label)
        {
            var prefab = Resources.Load<InputFieldBehaviour>($"{_resourcePathPrimitives}/InspectorInputFieldPrefab");
            var instance = Object.Instantiate(prefab);
            instance.Label = label;
            return instance.gameObject.AddComponent<InputFieldStringBehaviour>();;
        }

        public static ToggleBehaviour Toggle(string label)
        {
            var prefab = Resources.Load<ToggleBehaviour>($"{_resourcePathPrimitives}/InspectorTogglePrefab");
            var instance = Object.Instantiate(prefab);

            instance.Label = label;
        
            return instance;
        }
    
        public static DropDownBehaviour DropDown(string label)
        {
            var prefab = Resources.Load<DropDownBehaviour>($"{_resourcePathPrimitives}/InspectorDropDownPrefab");
            var instance = Object.Instantiate(prefab);

            instance.SetLabel(label);
        
            return instance;
        }

        public static ButtonBehaviour Button(string label)
        {
            var prefab = Resources.Load<ButtonBehaviour>($"{_resourcePathPrimitives}/InspectorButtonPrefab");
            var instance = Object.Instantiate(prefab);

            instance.Label = label;
        
            return instance;
        }

        private static IInspectorBehaviour GetCustomInspectorInstance(Type type, Transform content)
        {
            foreach (var customInspector in RundoEngine.ReflectionService.GetCustomInspectors())
            {
                if (customInspector.Item2.Type.IsAssignableFrom(type))
                {
                    if (_customInspectorPrefabs == null)
                    {
                        _customInspectorPrefabs = new List<IInspectorBehaviour>();
                        foreach (var prefab in Resources.LoadAll<GameObject>(_resourcePathCustom))
                        {
                            if (prefab.TryGetComponent<IInspectorBehaviour>(out var customInspectorBehaviour))
                                _customInspectorPrefabs.Add(customInspectorBehaviour);
                        }
                    }
                        
                    // prefab exists for component type
                    foreach (var prefab in _customInspectorPrefabs)
                        if (prefab.GameObject.GetComponent(customInspector.Item1) != null)
                            return Object.Instantiate(prefab.GameObject, content).GetComponent<IInspectorBehaviour>();
                        
                    // create new go
                    var go = new GameObject($"CustomInspector-{type.Name}");
                    go.AddComponent<RectTransform>();
                    go.AddComponent(customInspector.Item1);
                    go.transform.SetParent(content);
                    return go.GetComponent<IInspectorBehaviour>();
                }
            }

            return null;
        }

        public static void DrawInspector(DataHandler dataHandler, Transform content, bool showDefaultHeader = true)
        {
            var dataType = dataHandler.GetDataType();

            // data is of list type
            if (ReflectionUtils.IsList(dataType))
            {
                var inspectorListPrefab = Resources.Load<DefaultDataListInspectorBehaviour>($"{_resourcePathDefault}/InspectorListPrefab");
                var inspectorListInstance = Object.Instantiate(inspectorListPrefab, content);
                inspectorListInstance.SetData(dataHandler, dataHandler.GetLastMemberName());
            }
            // data is object/struct or a primitive
            else
            {
                // custom inspector exists
                var inspectorCustom = GetCustomInspectorInstance(dataType, content);
                if (inspectorCustom != null)
                {
                    inspectorCustom.SetData(dataHandler, dataHandler.GetLastMemberName());
                }
                // use default inspector
                else
                {
                    // add header with the prop name
                    if (showDefaultHeader)
                    {
                        var header =
                            Object.Instantiate(
                                Resources.Load<InspectorComplexDataHolderBehaviour>($"{_resourcePathDefault}/InspectorComplexDataHolderPrefab"), content);
                        header.SetLabel(dataHandler.GetLastMemberName());
                        content = header.Content;
                    }

                    // draw inspector into header content
                    var inspectorObjectPrefab = Resources.Load<DefaultDataInspectorBehaviour>($"{_resourcePathDefault}/InspectorObjectPrefab");
                    var inspectorObjectInstance = Object.Instantiate(inspectorObjectPrefab.GameObject, content).GetComponent<IInspectorBehaviour>();
                    inspectorObjectInstance.SetData(dataHandler, "");
                }
            }
        }
    }
}

