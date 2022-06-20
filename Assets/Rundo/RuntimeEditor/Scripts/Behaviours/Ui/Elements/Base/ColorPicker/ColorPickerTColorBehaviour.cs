using System;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data.UiDataMapper;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class ColorPickerTColorBehaviour : ColorPickerDataBehaviour<TColor>
    {
        public override TColor Value => ColorPickerElementBehaviour.GetColor();

        public override void OnSubmit(Action<UiDataMapperElementValue<TColor>> onSubmit)
        {
            ColorPickerElementBehaviour.OnColorChanged = (value) =>
            {
                onSubmit.Invoke(new UiDataMapperElementValue<TColor>(value, new UiDataMapperElementValueMetaData(true, 0 ,0)));
            };
            ColorPickerElementBehaviour.OnColorConfirmed = (value) =>
            {
                onSubmit.Invoke(new UiDataMapperElementValue<TColor>(value));
            };
        }

        protected override void SetValueInternal(TColor value)
        {
            ColorPickerElementBehaviour.SetColor(value);
        }
    }
}
