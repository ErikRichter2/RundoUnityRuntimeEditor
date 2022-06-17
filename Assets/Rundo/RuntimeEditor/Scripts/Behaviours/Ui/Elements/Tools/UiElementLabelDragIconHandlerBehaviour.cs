using Rundo.RuntimeEditor.Behaviours;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rundo.Ui
{
    public class UiElementLabelDragIconHandlerBehaviour : EditorBaseBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private bool _isPointerDown;
        private Transform _parent;
        private IUiDataMapperElementValueChangeableByCursorDragBehaviour _pointerDownElement;
        
        private void Start()
        {
            _parent = transform.parent;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            DispatchUiEvent(new CursorIconBehaviour.CursorIconEvent{Icon = "CursorDragIcon"});
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isPointerDown == false)
                DispatchUiEvent(new CursorIconBehaviour.CursorIconEvent());
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isPointerDown == false)
            {
                _isPointerDown = true;
                _pointerDownElement = GetComponentInParent<IUiDataMapperElementValueChangeableByCursorDragBehaviour>();
                _pointerDownElement?.OnRaycasterPointerDown();

                var canvas = GetComponentInParent<Canvas>();
                var rootCanvas = canvas.rootCanvas;
                transform.SetParent(rootCanvas.transform, false);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isPointerDown)
            {
                _isPointerDown = false;
                _pointerDownElement?.OnRaycasterPointerUp();
                _pointerDownElement = null;
                transform.SetParent(_parent, false);
            }
        }
        

    }
}

