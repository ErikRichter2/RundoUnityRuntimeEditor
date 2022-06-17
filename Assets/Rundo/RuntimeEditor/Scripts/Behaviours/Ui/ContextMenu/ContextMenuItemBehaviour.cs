using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    public class ContextMenuItemBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject MouseOverElement;
        public Button Button;
        public TextMeshProUGUI Text;

        private bool _isDisabled;
        
        public bool IsMouseOver { get; private set; }

        public void SetData<T>(ContextMenuItemData<T> data)
        {
            data.GameObject = gameObject;
            Text.text = data.Name;
            SetDisabled(data.Disabled);
            
            if (data.Disabled == false)
                Button.onClick.AddListener(() =>
                {
                    data.Callback?.Invoke(data.Data);
                    Destroy(GetComponentInParent<ContextMenuBehaviour>().gameObject);
                });
        }
        
        private void SetDisabled(bool value)
        {
            _isDisabled = value;
            if (_isDisabled)
            {
                var c = Text.color;
                c.a = 0.4f;
                Text.color = c;
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isDisabled)
                return;

            IsMouseOver = true;
            MouseOverElement.gameObject.SetActive(true);
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isDisabled)
                return;
            
            IsMouseOver = false;
            MouseOverElement.gameObject.SetActive(false);
        }
    }
}

