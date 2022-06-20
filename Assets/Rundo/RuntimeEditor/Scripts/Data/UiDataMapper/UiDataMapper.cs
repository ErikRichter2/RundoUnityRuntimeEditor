using System;
using System.Collections.Generic;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Behaviours;
using Rundo.RuntimeEditor.Factory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Data.UiDataMapper
{
    public class UiDataMapper
    {
        private readonly List<UiDataMapperElementInstance> _uiDataMapperElementInstances =
            new List<UiDataMapperElementInstance>();

        public Transform UiElementsContent { get; private set; }
        
        public DataHandler DataHandler { get; private set; }

        private Action _onDataChange;

        public void OnRootDataChange(Action onDataChange)
        {
            _onDataChange = onDataChange;
            DataHandler.OnRootDataChange(_onDataChange);
        }

        public void ClearElements()
        {
            _uiDataMapperElementInstances.Clear();
        }
        
        public void Clear()
        {
            DataHandler?.RemoveListeners();
            ClearElements();
        }

        public void SetDataHandler(DataHandler dataHandler)
        {
            DataHandler?.RemoveListeners();
            DataHandler = dataHandler;
            DataHandler.OnRootDataChange(_onDataChange);
        }

        /**
         * Sets the values from data into the mapped UI elements
         */
        public void Redraw()
        {
            foreach (var uiDataMapperElementInstance in _uiDataMapperElementInstances)
                uiDataMapperElementInstance.Redraw();
        }

        public void SetUiElementsContent(Transform content)
        {
            UiElementsContent = content;
        }
        
        protected UiDataMapperElementInstance Instantiate(Type dataMapperType, IUiDataMapperElementBehaviour element)
        {
            // reuse if already exists
            foreach (var it in _uiDataMapperElementInstances)
                if (it.IsElement(element))
                {
                    if (it.GetType() == dataMapperType)
                        return it;
                    throw new Exception($"Element {element} is already mapped to a different mapper instance type {it}");
                }

            var isDataMapperWithValue =
                typeof(IUiDataMapperElementInstanceWithValue).IsAssignableFrom(dataMapperType);

            UiDataMapperElementInstance uiDataMapperElementInstance = null;

            if (isDataMapperWithValue)
            {
                uiDataMapperElementInstance =
                    (UiDataMapperElementInstance)RundoEngine.DataFactory.Instantiate(dataMapperType, null,
                        new object[] { this, element });
            }
            else
            {
                uiDataMapperElementInstance =
                    (UiDataMapperElementInstance)RundoEngine.DataFactory.Instantiate(dataMapperType, null,
                        new object[] { element });
            }

            if (element is MonoBehaviour monoBehaviour)
                if (monoBehaviour.TryGetComponent<ICssElement>(out var cssElement))
                    cssElement.GetOrCreateCss();

            _uiDataMapperElementInstances.Add(uiDataMapperElementInstance);

            return uiDataMapperElementInstance;
        }

        /**
         * Gets or creates UiDataMapper wrapper instance for input IUiDataMapperElementBehaviour component.
         */
        protected TDataMapper Instantiate<TDataMapper>(IUiDataMapperElementBehaviour element) where TDataMapper: UiDataMapperElementInstance
        {
            var res = (TDataMapper)Instantiate(typeof(TDataMapper), element);
            return res;
        }
        
        protected static T AddDataMapperComponentIfNotExists<T>(GameObject gameObject) where T: MonoBehaviour
        {
            var c = gameObject.GetComponent<T>();
            c ??= gameObject.AddComponent<T>();
            return c;
        }

        public UiDataMapperElementInstance GetElementInstanceByName(string name)
        {
            foreach (var it in _uiDataMapperElementInstances)
                if (it.Name == name)
                    return it;
            return null;
        }
        
        public UiDataMapperElementInstance CreatePrimitive(Type valueType, string label, string name)
        {
            if (UiElementsContent == null)
                throw new Exception($"Need to call SetUiElementsContent() before Create");
            
            var element = UiFactory.InstantiatePrimitive(valueType, label);
            
            if (element == null)
                return null;
            
            if (element is MonoBehaviour monoBehaviour)
            {
                monoBehaviour.transform.SetParent(UiElementsContent);

                Type dataMapperType = null;

                if (element is ICustomUiDataMapper customUiDataMapper)
                    dataMapperType = customUiDataMapper.GetDataMapperType();

                if (monoBehaviour.TryGetComponent<ICssElement>(out var cssElement))
                    cssElement.GetOrCreateCss();

                dataMapperType ??= typeof(UiDataMapperElementInstance<>).MakeGenericType(new Type[] { valueType });
                
                var instance = Instantiate(dataMapperType, element);
                instance.SetName(name);
                return instance;
            }

            throw new Exception($"Cannot create ui element for type {valueType.Name}");
        }

        public UiDataMapperElementInstance<TValue> CreatePrimitive<TValue>(string label, string name = null)
        {
            if (string.IsNullOrEmpty(name))
                name = label;
            
            return CreatePrimitive(typeof(TValue), label, name) as UiDataMapperElementInstance<TValue>;
        }

        public UiDataMapperButtonInstance CreateButton(string label)
        {
            if (UiElementsContent == null)
                throw new Exception($"Need to call SetUiElementsContent() before Create");
            
            var instance = UiFactory.Button(label);
            
            if (instance == null)
                return null;
            
            if (instance is MonoBehaviour monoBehaviour)
            {
                monoBehaviour.transform.SetParent(UiElementsContent);

                Type dataMapperType = typeof(UiDataMapperElementInstance);

                if (instance is ICustomUiDataMapper customUiDataMapper)
                    dataMapperType = customUiDataMapper.GetDataMapperType();

                if (monoBehaviour.TryGetComponent<ICssElement>(out var cssElement))
                    cssElement.GetOrCreateCss();

                return (UiDataMapperButtonInstance)Instantiate(dataMapperType, instance);
            }

            return null;
        }

        /**
         * General map function that creates a UiDatMapper instance that wraps IUiDataMapperElementBehaviour component.
         * It should be used for custom-made behaviours that could implement IUiDataMapperElementBehaviour by default.
         */
        public UiDataMapperElementInstance<TValue> Map<TValue>(IUiDataMapperElementBehaviour<TValue> element)
        {
            return Instantiate<UiDataMapperElementInstance<TValue>>(element);
        }
        


        // these are mappings for Unity Components - or any components, where we don't have implemented an
        // IUiDataMapperElementBehaviour implicitly. Therefore we need to add a new component of IUiDataMapperElementBehaviour
        // type to input behaviour.
        
        /**
         * Maps Button
         */
        public UiDataMapperButtonInstance Map(Button button)
        {
            return Instantiate<UiDataMapperButtonInstance>(AddDataMapperComponentIfNotExists<UiDataMapperButtonElementBehaviour>(button.gameObject));
        }
        
        /**
         * Maps Toggle to a bool data value
         */
        public UiDataMapperElementInstance<bool> Map(Toggle toggle)
        {
            return Map(AddDataMapperComponentIfNotExists<UiDataMapperToggleElementBehaviour>(toggle.gameObject));
        }
        
        /**
         * Maps TMP_Dropdown to a int data value
         */
        public UiDataMapperElementInstance<int> Map(TMP_Dropdown dropDown)
        {
            return Map(AddDataMapperComponentIfNotExists<UiDataMapperDropDownElementBehaviour>(dropDown.gameObject));
        }
        
        public UiDataMapperElementInstance<TEnumType> Map<TEnumType>(DropDownBehaviour dropDown) where TEnumType: Enum
        {
            dropDown.FillFromEnum<TEnumType>();
            return Instantiate<UiDataMapperElementInstance<TEnumType>>(dropDown);
        }

        /**
         * Maps TMP_InputField to a string data value
         */
        public UiDataMapperElementInstance<string> Map(TMP_InputField inputField)
        {
            return Map(inputField.gameObject.AddComponent<UiDataMapperTMPInputFieldStringElementBehaviour>());
        }

        /**
         * Maps TMP_InputField to a TValue data value.
         */
        public UiDataMapperElementInstance<TValue> Map<TValue>(TMP_InputField inputField)
        {
            // int
            if (typeof(TValue) == typeof(int))
                return Map((IUiDataMapperElementBehaviour<TValue>)inputField.gameObject.AddComponent<UiDataMapperTMPInputFieldIntElementBehaviour>());
            
            // float
            if (typeof(TValue) == typeof(float))
                return Map((IUiDataMapperElementBehaviour<TValue>)inputField.gameObject.AddComponent<UiDataMapperTMPInputFieldFloatElementBehaviour>());

            // string
            if (typeof(TValue) == typeof(float))
                return Map((IUiDataMapperElementBehaviour<TValue>)inputField.gameObject.AddComponent<UiDataMapperTMPInputFieldStringElementBehaviour>());

            throw new Exception($"Mapping TMP_InputField to a type {nameof(TValue)} is not implemented.");
        }
    }
}
