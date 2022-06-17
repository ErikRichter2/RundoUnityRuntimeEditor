using System.Collections.Generic;
using System.Linq;
using Rundo.RuntimeEditor.Commands;
using Rundo.RuntimeEditor.Data;
using RuntimeHandle;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// Evidence of selected objects within the runtime editor, can render the transform gizmo over this objects.
    /// Object in the selection could be any object. Inspector window is in sync with this selection.
    /// </summary>
    public class SelectionBehaviour : EditorBaseBehaviour
    {
        public struct UnselectObjectEvent : IUiEvent {}
        public struct SelectObjectEvent : IUiEvent {}

        public struct SetTransformHandleType : IUiEvent
        {
            public HandleType HandleType;
        }
        
        private bool _isTransformHandlerDragging;
        private RuntimeTransformHandle _runtimeTransformHandle;
        private readonly List<object> _selection = new List<object>();
        private readonly List<GameObject> _transformableSelection = new List<GameObject>();
        private readonly List<SelectionTransformHandleSync> _transformSyncData = new List<SelectionTransformHandleSync>();
        private GameObject _runtimeTransformHandleMultiTarget;

        public bool IsTransformHandleDragging =>
            _runtimeTransformHandle.gameObject.activeSelf && _runtimeTransformHandle.IsDragging;

        private void Start()
        {
            _runtimeTransformHandleMultiTarget = new GameObject("SelectionContainer");
            _runtimeTransformHandleMultiTarget.transform.SetParent(transform);
            _runtimeTransformHandle = RuntimeTransformHandle.Create(_runtimeTransformHandleMultiTarget.transform, HandleType.POSITION);
            _runtimeTransformHandle.transform.SetParent(transform, true);
            _runtimeTransformHandle.gameObject.name = "RuntimeTransformHandle";
            _runtimeTransformHandle.gameObject.SetActive(false);
            _runtimeTransformHandle.snappingType = HandleSnappingType.ABSOLUTE;
            
            RegisterCommandListener<DestroyDataGameObjectCommand>(OnDestroyDataGameObjectCommand);
            RegisterCommandListener<DataTransformBehaviour>(RefreshRuntimeTransformHandlePosition);

            RegisterUiEvent<SetTransformHandleType>(OnSetTransformHandleType);
        }

        private void OnSetTransformHandleType(SetTransformHandleType data)
        {
            _runtimeTransformHandle.type = data.HandleType;
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                _runtimeTransformHandle.positionSnap = Vector3.one;
            }
            else
            {
                _runtimeTransformHandle.positionSnap = Vector3.zero;
            }

            // drag start
            if (_isTransformHandlerDragging == false && _runtimeTransformHandle.IsDragging)
            {
                _isTransformHandlerDragging = true;
                
                foreach (var it in _transformSyncData)
                    it.StopDrag();
                _transformSyncData.Clear();
                
                foreach (var it in _transformableSelection)
                    _transformSyncData.Add(new SelectionTransformHandleSync(_runtimeTransformHandle, it));
            }
            // drag end - commit
            else if (_isTransformHandlerDragging && _runtimeTransformHandle.IsDragging == false)
            {
                _isTransformHandlerDragging = false;
                foreach (var it in _transformSyncData)
                    it.StopDrag();
                _transformSyncData.Clear();
            }
            // drag ongoing
            else if (_isTransformHandlerDragging)
            {
                foreach (var it in _transformSyncData)
                    it.ProcessDrag();
            }
        }

        private void OnDestroyDataGameObjectCommand(DestroyDataGameObjectCommand data)
        {
            RemoveFromSelection(data.DataGameObject);
        }

        public void RemoveFromSelection(object obj)
        {
            RemoveFromSelectionInternal(obj);
        }

        private void RemoveFromSelectionInternal(object obj)
        {
            if (obj is DataGameObject dataGameObject)
            {
                var behaviour = RuntimeEditorController.DataSceneBehaviour.Find(dataGameObject);

                if (behaviour != null)
                {
                    if (behaviour.TryGetComponent<EditorRaycastHitColliderHandlerBehaviour>(
                        out var raycastHitColliderHandlerBehaviour))
                        raycastHitColliderHandlerBehaviour.SelectionState =
                            EditorRaycastHitColliderHandlerBehaviour.SelectionStateEnum.None;
                
                    _transformableSelection.Remove(behaviour.gameObject);
                }
            }

            _selection.Remove(obj);
            DispatchUiEvent(new UnselectObjectEvent());
            RefreshRuntimeTransformHandlePosition();
        }

        public void AddToSelection(object obj)
        {
            if (IsSelected(obj))
            {
                RemoveFromSelection(obj);
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftControl) == false && 
                    Input.GetKey(KeyCode.RightControl) == false)
                {
                    ClearSelection();
                }
                    
                AddToSelectionInternal(obj);
            }
        }

        private void AddToSelectionInternal(object obj)
        {
            _selection.Add(obj);

            if (obj is DataGameObject dataGameObject)
            {
                var behaviour = RuntimeEditorController.DataSceneBehaviour.Find(dataGameObject);
                if (behaviour != null)
                {
                    if (behaviour.TryGetComponent<EditorRaycastHitColliderHandlerBehaviour>(
                        out var raycastHitColliderHandlerBehaviour))
                        raycastHitColliderHandlerBehaviour.SelectionState =
                            EditorRaycastHitColliderHandlerBehaviour.SelectionStateEnum.Selected;
                    if (dataGameObject.GetComponent<DataTransformBehaviour>() != null &&
                        dataGameObject.GetComponent<DataTransformBehaviour>().IsReadOnly() == false)
                    {
                        _transformableSelection.Add(behaviour.gameObject);
                    }
                }
            }

            DispatchUiEvent(new SelectObjectEvent());
            RefreshRuntimeTransformHandlePosition();
        }

        public void AddToSelectionWithoutTransformGizmo(object obj)
        {
            _selection.Add(obj);
            DispatchUiEvent(new SelectObjectEvent());
        }

        public void RefreshRuntimeTransformHandlePosition()
        {
            _runtimeTransformHandle.gameObject.SetActive(_transformableSelection.Count > 0);

            if (_transformableSelection.Count <= 0)
                return;

            if (_transformableSelection.Count == 1)
            {
                _runtimeTransformHandle.target = _transformableSelection[0].transform;
            }
            else
            {
                _runtimeTransformHandle.target = _runtimeTransformHandleMultiTarget.transform;

                var selectionPositions = new List<Vector3>();

                foreach (var it in _transformableSelection)
                    selectionPositions.Add(it.GetComponent<DataGameObjectBehaviour>().DataGameObject.GetComponent<DataTransformBehaviour>().Data.Position);
            
                if (selectionPositions.Count <= 0)
                    return;
         
                var center = GeometryUtility.CalculateBounds(selectionPositions.ToArray(), Matrix4x4.identity).center;
                _runtimeTransformHandle.target.position = center;
                _runtimeTransformHandle.target.rotation = Quaternion.identity;
                _runtimeTransformHandle.target.localScale = Vector3.one;
            }
        }

        public void ClearSelection()
        {
            while (_selection.Count > 0)
                RemoveFromSelection(_selection.First());
        }

        public bool IsSelected(object obj)
        {
            return _selection.Contains(obj);
        }

        public List<object> GetSelectionData()
        {
            return _selection;
        }
    }

}


