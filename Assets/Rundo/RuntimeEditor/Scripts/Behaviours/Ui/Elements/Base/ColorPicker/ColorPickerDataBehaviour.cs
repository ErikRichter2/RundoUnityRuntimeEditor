using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    [RequireComponent(typeof(ColorPickerElementBehaviour))]
    public abstract class ColorPickerDataBehaviour<TColorType> : UiDataMapperElementBehaviour<TColorType>
    {
        private ColorPickerElementBehaviour _colorPickerElementBehaviour;

        protected ColorPickerElementBehaviour ColorPickerElementBehaviour
        {
            get
            {
                _colorPickerElementBehaviour ??= GetComponent<ColorPickerElementBehaviour>();
                return _colorPickerElementBehaviour;
            }
        }

        protected override void SetUndefinedValue()
        {
            ColorPickerElementBehaviour.SetColor(Color.black);
        }
        
        public override void InitDefaultCssValues(CssBehaviour cssBehaviour)
        {
            cssBehaviour.SetDefaultValue(CssPropertyEnum.LabelWidth,
                ColorPickerElementBehaviour.LabelElement.rectTransform.sizeDelta.x);
        }

        public override void UpdateCss(CssBehaviour cssBehaviour)
        {
            if (cssBehaviour.TryGetFloat(CssPropertyEnum.LabelWidth, out var value))
                ColorPickerElementBehaviour.LabelElement.rectTransform.sizeDelta =
                    new Vector2(value, ColorPickerElementBehaviour.LabelElement.rectTransform.sizeDelta.y);
        }

    }
}
