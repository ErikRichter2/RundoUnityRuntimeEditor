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
        
        public static IUiDataMapperElementBehaviour GetUiDataMapperElement(Type valueType, string label)
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
                var dropDown = DrowDown(label);
                dropDown.FillFromEnum(valueType);
                return dropDown;
            }

            return default;
        }

        public static IUiDataMapperElementBehaviour<TValue> GetUiDataMapperElement<TValue>(string label)
        {
            return GetUiDataMapperElement(typeof(TValue), label) as IUiDataMapperElementBehaviour<TValue>;
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
    
        public static DropDownBehaviour DrowDown(string label)
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

        public static IInspectorBehaviour InstantiateInspectorPrefab(
            Type type, 
            Transform content,
            bool? showHeader = null,
            string headerName = null)
        {
            if (type == null)
                return null;

            var finalShowHeader = true;
            var finalHeaderName = type.Name;

            IInspectorBehaviour GetPrefab()
            {
                if (ReflectionUtils.IsList(type))
                    return Resources.Load<DefaultDataListInspectorBehaviour>($"{_resourcePathDefault}/InspectorListPrefab");

                foreach (var customInspector in RundoEngine.ReflectionService.GetCustomInspectors())
                {
                    if (customInspector.Item2.Type.IsAssignableFrom(type))
                    {
                        showHeader = customInspector.Item2.ShowHeader;
                        if (customInspector.Item2.HeaderName != null)
                            headerName = customInspector.Item2.HeaderName;
                        
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
                                return prefab;
                        
                        // create new go
                        var go = new GameObject($"CustomInspector-{type.Name}");
                        go.AddComponent<RectTransform>();
                        go.AddComponent(customInspector.Item1);
                        return go.GetComponent<IInspectorBehaviour>();
                    }
                }

                return Resources.Load<DefaultDataInspectorBehaviour>($"{_resourcePathDefault}/InspectorObjectPrefab");
            }
            
            var prefab = GetPrefab();

            if (showHeader != null)
                finalShowHeader = showHeader.Value;
            if (headerName != null)
                finalHeaderName = headerName;

            if (finalShowHeader)
            {
                var header =
                    Object.Instantiate(
                        Resources.Load<InspectorComplexDataHolderBehaviour>($"{_resourcePathDefault}/InspectorComplexDataHolderPrefab"), content);
                header.SetLabel(finalHeaderName);
                content = header.Content;
            }
            
            return Object.Instantiate(prefab.GameObject, content).GetComponent<IInspectorBehaviour>();
        }
    }

    public class UiDataMapperElementAttribute : Attribute
    {
        public readonly Type Type;
        
        public UiDataMapperElementAttribute(Type type)
        {
            Type = type;
        }
    }
}

