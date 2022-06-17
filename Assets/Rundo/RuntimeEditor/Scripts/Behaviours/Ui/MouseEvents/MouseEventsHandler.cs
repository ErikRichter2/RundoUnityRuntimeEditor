using UnityEngine;
using UnityEngine.EventSystems;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class MouseEventsHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private bool _isMouseOver;
        private bool _isMouseDown;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _isMouseOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isMouseOver = false;
        }

        private void Update()
        {
            if (_isMouseOver)
            {
                if (Input.GetMouseButtonUp(1))
                {
                    GetComponentInParent<IMouseEventsListener>()?.OnRightClick();
                }
            }
        }
    }
}
