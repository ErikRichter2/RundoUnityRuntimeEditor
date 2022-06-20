using Rundo.RuntimeEditor.Data;
using Rundo.RuntimeEditor.Factory;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// Handles icons by the mouse cursor.
    /// </summary>
    public class CursorIconBehaviour : EditorBaseBehaviour
    {
        public struct CursorIconEvent : IUiEvent
        {
            public string Icon;
        }
        
        private RectTransform _activeIcon;

        private void Start()
        {
            RegisterUiEvent<CursorIconEvent>(OnCursorIconEvent);
            HideMouseIcon();
        }

        private void OnCursorIconEvent(CursorIconEvent data)
        {
            if (string.IsNullOrEmpty(data.Icon))
                HideMouseIcon();
            else
                ShowMouseIcon(data.Icon);
        }

        public void ShowMouseIcon(string icon)
        {
            HideMouseIcon();
            _activeIcon = UiFactory.DrawMouseIcon(icon, transform);
        }

        public void HideMouseIcon()
        {
            if (_activeIcon == null)
                return;
            
            Destroy(_activeIcon.gameObject);
            _activeIcon = null;
        }

        private void LateUpdate()
        {
            if (_activeIcon != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    GetComponent<RectTransform>(), 
                    Input.mousePosition,
                    null, 
                    out var localPoint);

                _activeIcon.anchoredPosition = new Vector2(localPoint.x, localPoint.y);
            }
        }        
    }
}
