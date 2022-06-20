using System;
using Rundo.RuntimeEditor.Data.UiDataMapper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class ToggleBehaviour : UiDataMapperElementBehaviour<bool>
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Toggle _toggle;
        
        public override bool Value => _toggle.isOn;

        public string Label
        {
            get => _label.text;
            set => _label.text = value;
        }


        protected override void SetUndefinedValue()
        {
            _toggle.SetIsOnWithoutNotify(false);
        }

        public override void OnSubmit(Action<UiDataMapperElementValue<bool>> onSubmit)
        {
            _toggle.onValueChanged.AddListener(value => { onSubmit(new UiDataMapperElementValue<bool>(value)); });
        }

        protected override void SetValueInternal(bool value)
        {
            _toggle.SetIsOnWithoutNotify(value);
        }

        public override void InitDefaultCssValues(CssBehaviour cssBehaviour)
        {
            cssBehaviour.SetDefaultValue(CssPropertyEnum.LabelWidth,
                _label.rectTransform.sizeDelta.x);
        }

        public override void UpdateCss(CssBehaviour cssBehaviour)
        {
            if (cssBehaviour.TryGetFloat(CssPropertyEnum.LabelWidth, out var value))
                _label.rectTransform.sizeDelta =
                    new Vector2(value, _label.rectTransform.sizeDelta.y);
        }

    }
}


