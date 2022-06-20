using System;
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

        public TMP_InputField InputField => _inputField;

        protected override void SetUndefinedValue()
        {
            _inputField.SetTextWithoutNotify("--");
        }

        public override void OnSubmit(Action<UiDataMapperElementValue<string>> onSubmit)
        {
            _inputField.onSubmit.AddListener((value) => onSubmit.Invoke(new UiDataMapperElementValue<string>(value)));
        }

        protected override void SetValueInternal(string value)
        {
            _inputField.SetTextWithoutNotify(value);
        }

        public bool ReadOnly
        {
            get => _inputField.readOnly;
            set => _inputField.readOnly = value;
        }

    }
}


