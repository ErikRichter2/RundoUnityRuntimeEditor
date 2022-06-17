using UnityEngine.EventSystems;

namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// Sets the static IsInputOverWorld property if the mouse is over UI elements, or over the world.
    /// </summary>
    public class WorldInputsBehaviour : EditorBaseBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            RuntimeEditorBehaviour.IsInputOverWorld = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            RuntimeEditorBehaviour.IsInputOverWorld = false;
        }
    }
}


