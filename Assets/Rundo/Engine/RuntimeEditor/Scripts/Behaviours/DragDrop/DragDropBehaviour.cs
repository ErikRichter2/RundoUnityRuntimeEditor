using Rundo.Core.Utils;
using Rundo.RuntimeEditor.Utils;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class DragDropBehaviour : EditorBaseBehaviour, IIgnoreRayCast
    {
        public object Data { get; private set; }
        public IDragDropHandler CurrentHitTarget { get; private set; }
        
        public void SetData(object data)
        {
            DispatchUiEvent(new CursorIconBehaviour.CursorIconEvent{Icon = "CursorDragDropInvalidIcon"});
            Data = data;
        }

        private void Update()
        {
            RectTransformUtils.SnapToCursor(GetComponentInParent<Canvas>().GetComponent<RectTransform>(), GetComponent<RectTransform>());
            
            RefreshDragDropHandler();
            
            if (Input.GetMouseButtonUp(0))
            {
                DispatchUiEvent(new CursorIconBehaviour.CursorIconEvent());
                Destroy(gameObject);
            }
        }
        
        private void RefreshDragDropHandler()
        {
            CurrentHitTarget = null;
            
            DispatchUiEvent(new CursorIconBehaviour.CursorIconEvent{Icon = "CursorDragDropInvalidIcon"});
            
            foreach (var it in RaycastUtils.RaycastUi())
            {
                var handler = it.gameObject.GetComponentInParent<IDragDropHandler>();
                if (handler != null)
                {
                    if (handler.CanHandleDragDrop(this))
                    {
                        CurrentHitTarget = handler;
                        DispatchUiEvent(new CursorIconBehaviour.CursorIconEvent{Icon = "CursorDragDropValidIcon"});

                        if (Input.GetMouseButtonUp(0))
                            handler.HandleDragDrop(this);

                        return;
                    }
                }
            }
        }
    }
}

