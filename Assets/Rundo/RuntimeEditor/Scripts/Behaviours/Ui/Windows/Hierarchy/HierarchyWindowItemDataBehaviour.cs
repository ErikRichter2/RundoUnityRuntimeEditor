using Rundo.RuntimeEditor.Data;
using TMPro;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    public class HierarchyWindowItemDataBehaviour : HierarchyWindowItemBaseBehaviour
    {
        [SerializeField] private TMP_Text _name;
        [SerializeField] private ExpandCollapseButtonBehaviour _expandedCollapseButton;

        private float _timeSinceMouseDown;
        private bool _isMouseDown;
        private bool _readOnly;
        private bool _expandedCollapsedHidden;

        private void Start()
        {
            RegisterUiEvent<EditorUiBehaviour.SetHierarchyExpandedStateEvent>(
                RefreshExpandedCollapsed);
            
            RefreshExpandedCollapsed();
            
            _expandedCollapseButton.OnClick(OnExpandedCollapsedClick);
        }

        private void OnExpandedCollapsedClick()
        {
            DispatchUiEvent(new EditorUiBehaviour.SetHierarchyExpandedStateEvent
            {
                IsExpanded = _expandedCollapseButton.IsExpanded,
                DataGameObjectId = DataGameObject.ObjectId
            });
        }

        public void HideExpandedCollapsedButtons()
        {
            _expandedCollapsedHidden = true;
            _expandedCollapseButton.gameObject.SetActive(false);
        }

        private void RefreshExpandedCollapsed()
        {
            _expandedCollapseButton.gameObject.SetActive(false);

            if (_expandedCollapsedHidden)
                return;
            if (DataGameObject == null)
                return;
            if (DataGameObject.Children.Count == 0)
                return;
            
            var isExpanded =
                RuntimeEditorBehaviour.PersistentEditorPrefs.LoadData().ExpandedDataGameObjectsInHierarchyWindow.Contains(DataGameObject.ObjectId);
            
            _expandedCollapseButton.gameObject.SetActive(true);

            if (isExpanded)
                _expandedCollapseButton.Expand();
            else
                _expandedCollapseButton.Collapse();
        }

        private void Update()
        {
            if (_readOnly)
                return;
            
            if (_isMouseDown == false && _isMouseOver && Input.GetMouseButtonDown(0))
            {
                _timeSinceMouseDown = 0f;
                _isMouseDown = true;
            }

            if (_isMouseDown && Input.GetMouseButton(0) == false)
            {
                if (_timeSinceMouseDown <= 0.2f)
                    RuntimeEditorController.SelectionBehaviour.AddToSelection(DataGameObject);
                _isMouseDown = false;
            }

            if (_isMouseDown)
            {
                _timeSinceMouseDown += Time.deltaTime;
                if (_timeSinceMouseDown > 0.2f)
                {
                    _isMouseDown = false;
                    _timeSinceMouseDown = 0f;
                    GetComponentInParent<HierarchyWindowBehaviour>().StartDrag(this);
                }
            }
        }

        public void SetReadOnly()
        {
            _readOnly = true;
        }

        protected override void SetDataInternal()
        {
            _name.text = DataGameObject.Name;
        }
        
        public override bool CanHandleDragDrop(DragDropBehaviour dropBehaviour)
        {
            if (dropBehaviour.Data is DataGameObject dataGameObject)
                if (dataGameObject != DataGameObject)
                {
                    if (RuntimeEditorBehaviour.PersistentEditorPrefs.LoadData().ExpandedDataGameObjectsInHierarchyWindow.Contains(
                        DataGameObject.ObjectId) == false)
                    {
                        DispatchUiEvent(new EditorUiBehaviour.SetHierarchyExpandedStateEvent
                        {
                            IsExpanded = true,
                            DataGameObjectId = DataGameObject.ObjectId
                        });
                    }
                    
                    return true;
                }
            return false;
        }

        protected override void ProcessDragDrop(DataGameObject dataGameObject)
        {
            dataGameObject.SetDataGameObjectParent(DataGameObject);
        }
    }
}
