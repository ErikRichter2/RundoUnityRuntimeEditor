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

        public override void SetUndefValue()
        {
            GetComponent<InputFieldBehaviour>().SetUndefValue();
        }
        
        public override void OnSubmit(Action<UiDataMapperElementValue<TValue>> onSubmit)
        {
            _onSubmitValueData = onSubmit;
            GetComponent<InputFieldBehaviour>().OnSubmit(value => InvokeSubmit(false));
        }

        public override void SetValue(TValue value)
        {
            GetComponent<InputFieldBehaviour>().Text = value.ToString();
        }

        public void OnRaycasterPointerUp()
        {
            if (IsMouseDragAvailable == false)
                return;
            
            var currentValue = Value;
            _isPointerDown = false;
            SetValue(_pointerDownValue);
            InvokeSubmit(true);
            SetValue(currentValue);
            InvokeSubmit(false);
        }

        public void OnRaycasterPointerDown()
        {
            if (IsMouseDragAvailable == false)
                return;
            
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
                SetValue(newValue);
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


