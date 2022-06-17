using System;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data.UiDataMapper;
using TMPro;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class InputFieldBehaviour : UiDataMapperElementBehaviour<string>
    {
        [SerializeField] private RectTransform _labelElement;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_InputField _inputField;

        public RectTransform LabelElement => _labelElement;
        
        public string Label
        {
            get => _label.text;
            set
            {
                _label.text = value;
                _labelElement.gameObject.SetActive(!string.IsNullOrEmpty(value));
            }
        }

        public string Text
        {
            get => _inputField.text;
            set => _inputField.text = value;
        }

        public IUiDataMapperElementBehaviour<TValue> GetIUiDataMapperElementBehaviour<TValue>()
        {
            // int
            if (typeof(TValue) == typeof(int))
                return (IUiDataMapperElementBehaviour<TValue>)_inputField.gameObject.AddComponent<UiDataMapperTMPInputFieldIntElementBehaviour>();
            
            // float
            if (typeof(TValue) == typeof(float))
                return (IUiDataMapperElementBehaviour<TValue>)_inputField.gameObject.AddComponent<UiDataMapperTMPInputFieldFloatElementBehaviour>();

            // string
            if (typeof(TValue) == typeof(string))
                return (IUiDataMapperElementBehaviour<TValue>)_inputField.gameObject.AddComponent<UiDataMapperTMPInputFieldStringElementBehaviour>();

            throw new Exception($"Mapping TMP_InputField to a type {typeof(TValue).Name} is not implemented.");
        }

        public void Select()
        {
            
        }

        public void OnValueChanged(Action<string> onValueChanged)
        {
            _inputField.onValueChanged.AddListener(onValueChanged.Invoke);
        }
/*
        public void OnSubmit(Action<string> onSubmit)
        {
            _inputField.onSubmit.AddListener(onSubmit.Invoke);
        }
*/
        public override void OnSubmit(Action<UiDataMapperElementValue<string>> onSubmit)
        {
            _inputField.onSubmit.AddListener((value) => onSubmit.Invoke(new UiDataMapperElementValue<string>(value)));
        }

        public override void SetValue(string value)
        {
            IsUndefValue = false;
            _inputField.SetTextWithoutNotify(value);
        }

        public override string Value => _inputField.text;

        public bool ReadOnly
        {
            get => _inputField.readOnly;
            set => _inputField.readOnly = value;
        }

        public override void SetUndefValue()
        {
            IsUndefValue = true;
            _inputField.SetTextWithoutNotify("--");
        }
    }
}


