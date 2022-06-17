using System;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data.UiDataMapper;

namespace Rundo.RuntimeEditor.Behaviours
{
    public abstract class UiDataMapperElementBehaviour<TValue> : EditorBaseBehaviour, IUiDataMapperElementBehaviour<TValue>, ICssElement
    {
        private Func<TValue, object> _fromUiToDataConverter;
        private Func<object, TValue> _fromDataToUiConverter;
        
        public virtual TValue Value { get; protected set; }
        public bool IsUndefValue { get; set; }

        public abstract void OnSubmit(Action<UiDataMapperElementValue<TValue>> onSubmit);
        public abstract void SetValue(TValue value);
        public abstract void SetUndefValue();

        public virtual void SetDynamicValue(object value)
        {
            if (_fromDataToUiConverter != null)
                value = _fromDataToUiConverter.Invoke(value);
            
            if (this is IUiDataMapperDynamicValuesCustomHandler)
                throw new Exception($"Override {nameof(SetDynamicValue)} when implementing {nameof(IUiDataMapperDynamicValuesCustomHandler)}");

            if (value is TValue valueTyped)
                SetValue(valueTyped);
            else
                throw new Exception($"Cannot set value of type {value.GetType()}, expecting type {typeof(TValue).Name}");
        }

        public virtual void OnSubmitDynamicValue(Type expectedDataType, Action<UiDataMapperElementValue<object>> onSubmitDynamicValue)
        {
            if (this is IUiDataMapperDynamicValuesCustomHandler)
                throw new Exception($"Override {nameof(OnSubmitDynamicValue)} when implementing {nameof(IUiDataMapperDynamicValuesCustomHandler)}");

            OnSubmit(typedValue =>
            {
                object value = typedValue.Value;
                if (_fromUiToDataConverter != null)
                    value = _fromUiToDataConverter.Invoke(typedValue.Value);

                onSubmitDynamicValue.Invoke(new UiDataMapperElementValue<object>(value, typedValue.MetaData));
            });
        }

        public void SetValue(DataHandlerValue dataHandlerValue)
        {
            if (dataHandlerValue.IsUndefined || dataHandlerValue.Value == null)
                SetUndefValue();
            else
                SetDynamicValue(dataHandlerValue.Value);
        }

        public CssBehaviour GetOrCreateCss()
        {
            if (TryGetComponent<CssBehaviour>(out var cssBehaviour))
                return cssBehaviour;
            return gameObject.AddComponent<CssBehaviour>();
        }

        public virtual void InitDefaultCssValues(CssBehaviour cssBehaviour)
        {
            
        }

        public virtual void UpdateCss(CssBehaviour cssBehaviour)
        {
            
        }

        public void SetValueConverter(Func<TValue, object> fromUiToDataConverter, Func<object, TValue> fromDataToUiConverter)
        {
            _fromDataToUiConverter = fromDataToUiConverter;
            _fromUiToDataConverter = fromUiToDataConverter;
        }
    }
}
    