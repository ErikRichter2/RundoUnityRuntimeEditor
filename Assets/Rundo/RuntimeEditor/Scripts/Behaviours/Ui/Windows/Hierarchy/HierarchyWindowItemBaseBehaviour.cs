using Rundo.RuntimeEditor.Data;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Rundo.RuntimeEditor.Behaviours
{
    public abstract class HierarchyWindowItemBaseBehaviour : EditorBaseBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragDropHandler
    {
        [SerializeField] private RectTransform _offset;
        [SerializeField] private GameObject _manualSelection;
        [SerializeField] private GameObject _dragSelection;

        public DataGameObject DataGameObject { get; private set; }

        protected bool _isMouseOver;

        public void SetManualSelection(bool value)
        {
            if (_manualSelection != null)
                _manualSelection.SetActive(value);
        }

        public void SetDragSelection(bool value)
        {
            if (_dragSelection != null)
                _dragSelection.SetActive(value);
        }
        
        public void SetData(DataGameObject dataGameObject, int depth)
        {
            DataGameObject = dataGameObject;
            _offset.anchoredPosition = new Vector2(depth * 20f, 0f);
            SetDataInternal();
        }

        protected abstract void SetDataInternal();
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _isMouseOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isMouseOver = false;
        }

        public virtual bool CanHandleDragDrop(DragDropBehaviour dropBehaviour)
        {
            if (dropBehaviour.Data is DataGameObject)
                return true;
            return false;
        }

        public void HandleDragDrop(DragDropBehaviour dropBehaviour)
        {
            var dragDropData = dropBehaviour.Data as DataGameObject;
            Assert.IsNotNull(dragDropData);

            if (dragDropData == DataGameObject)
                return;

            if (this is HierarchyWindowItemDataBehaviour hierarchyViewItemDataBehaviour)
            {
                dragDropData.SetDataGameObjectParent(hierarchyViewItemDataBehaviour.DataGameObject);
            }
            else if (this is HierarchyWindowItemSeparatorBehaviour
                hierarchyViewItemSeparatorBehaviour)
            {
                var index = hierarchyViewItemSeparatorBehaviour.DataGameObject.GetDataGameObjectParent().GetCollection()
                    .IndexOf(hierarchyViewItemSeparatorBehaviour.DataGameObject);
                dragDropData.SetDataGameObjectParent(hierarchyViewItemSeparatorBehaviour.DataGameObject.GetDataGameObjectParent(), index);
            }
        }
    }
}
