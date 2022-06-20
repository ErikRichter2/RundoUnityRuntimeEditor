using System;
using Rundo.RuntimeEditor.Data.UiDataMapper;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class ColorPickerUnityColorBehaviour : ColorPickerDataBehaviour<Color>
    {
        public override Color Value => ColorPickerElementBehaviour.GetColor();

        public override void OnSubmit(Action<UiDataMapperElementValue<Color>> onSubmit)
        {
            ColorPickerElementBehaviour.OnColorChanged = (value) =>
            {
                onSubmit.Invoke(new UiDataMapperElementValue<Color>(value, new UiDataMapperElementValueMetaData(true, 0, 0)));
            };
            ColorPickerElementBehaviour.OnColorConfirmed = (value) =>
            {
                onSubmit.Invoke(new UiDataMapperElementValue<Color>(value));
            };
        }

        protected override void SetValueInternal(Color value)
        {
            ColorPickerElementBehaviour.SetColor(value);
        }
    }
}
