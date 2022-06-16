using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class EditorWorldRaycasterBehaviour : EditorBaseBehaviour
    {
        public EditorRaycastHitColliderHandlerBehaviour Raycast()
        {
            var ray = RuntimeEditorController.ActiveCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hitInfo))
                if (hitInfo.collider.gameObject != null)
                    return hitInfo.collider.GetComponentInParent<EditorRaycastHitColliderHandlerBehaviour>();

            return null;
        }
    }
}


