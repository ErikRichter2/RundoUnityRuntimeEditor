using System;
using Rundo.RuntimeEditor.Data.UiDataMapper;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    [RequireComponent(typeof(InputFieldBehaviour))]
    public abstract class InputFieldGenericValueBehaviour<TValue> : UiDataMapperElementBehaviour<TValue>
    {
        private bool _isPointerDown;
        private Vector2 _pointerDownMousePosition;
        private Vector2 _prevFrameMousePosition;
        private TValue _pointerDownValue;
        private Action<UiDataMapperElementValue<TValue>> _onSubmitValueData;

        protected override void SetUndefinedValue()
        {
            GetComponent<InputFieldBehaviour>().IsUndefinedValue = true;
        }

        public override void OnSubmit(Action<UiDataMapperElementValue<TValue>> onSubmit)
        {
            _onSubmitValueData = onSubmit;
            GetComponent<InputFieldBehaviour>().OnSubmit(value =>
            {
                Value = ValueFromString(value.Value);
                InvokeSubmit(false);
            });
        }

        protected override void SetValueInternal(TValue value)
        {
            GetComponent<InputFieldBehaviour>().Value = ValueToString(value);
        }

        protected abstract TValue ValueFromString(string value);
        protected abstract string ValueToString(TValue value);

        public void OnRaycasterPointerUp()
        {
            var currentValue = Value;
            _isPointerDown = false;
            Value = _pointerDownValue;
            InvokeSubmit(true);
            Value = currentValue;
            InvokeSubmit(false);
        }

        public void OnRaycasterPointerDown()
        {
            _isPointerDown = true;
            _pointerDownMousePosition = Input.mousePosition;
            _prevFrameMousePosition = _pointerDownMousePosition;
            _pointerDownValue = Value;
        }
        
        private void InvokeSubmit(bool ignoreUndoRedo)
        {
            _onSubmitValueData?.Invoke(new UiDataMapperElementValue<TValue>(Value, new UiDataMapperElementValueMetaData(ignoreUndoRedo, 0, 0)));
        }

        private void Update()
        {
            if (_isPointerDown)
            {
                var currentMousePos = Input.mousePosition;
                var delta = currentMousePos.x - _prevFrameMousePosition.x;
                _prevFrameMousePosition = currentMousePos;
                var newValue = GetMouseDragValue(delta);
                Value = newValue;
                InvokeSubmit(true);
            }
        }

        protected abstract TValue GetMouseDragValue(float delta);
        
        public override void InitDefaultCssValues(CssBehaviour cssBehaviour)
        {
            cssBehaviour.SetDefaultValue(CssPropertyEnum.LabelWidth,
                GetComponent<InputFieldBehaviour>().LabelElement.sizeDelta.x);
        }

        public override void UpdateCss(CssBehaviour cssBehaviour)
        {
            if (cssBehaviour.TryGetFloat(CssPropertyEnum.LabelWidth, out var value))
                GetComponent<InputFieldBehaviour>().LabelElement.sizeDelta =
                    new Vector2(value, GetComponent<InputFieldBehaviour>().LabelElement.sizeDelta.y);
        }

        public virtual bool IsMouseDragAvailable => false;
    }
}


