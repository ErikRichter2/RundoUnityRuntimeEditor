using System.Collections.Generic;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Commands;
using Rundo.RuntimeEditor.Data;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// Default editor mode - allows for objects selection and manipulation.
    /// </summary>
    public class SelectObjectsEditorModeBehaviour : EditorModeBaseBehaviour
    {
        private EditorWorldRaycasterBehaviour _worldRaycasterBehaviour;
        private EditorRaycastHitColliderHandlerBehaviour _raycastHitColliderHandler;

        private void Start()
        {
            _worldRaycasterBehaviour = gameObject.AddComponent<EditorWorldRaycasterBehaviour>();
        }
        
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Delete))
                foreach (var it in RuntimeEditorController.SelectionBehaviour.GetSelectionData().ToArray())
                    if (it is DataGameObject dataGameObject)
                        DestroyDataGameObjectCommand.Process(DataScene, dataGameObject);

            if (Input.GetKey(KeyCode.LeftControl) &&
                Input.GetKeyUp(KeyCode.C))
            {
                var copy = new List<DataGameObject>();
                foreach (var it in RuntimeEditorController.SelectionBehaviour.GetSelectionData().ToArray())
                    if (it is DataGameObject dataGameObject)
                        copy.Add(dataGameObject);
                
                Clipboard.Set(copy);
            }

            if (Input.GetKey(KeyCode.LeftControl) &&
                Input.GetKeyUp(KeyCode.V))
            {
                var cloned = Clipboard.CloneList<DataGameObject>();
                if (cloned != null && cloned.Count > 0)
                {
                    RuntimeEditorController.SelectionBehaviour.ClearSelection();
                    foreach (var it in cloned)
                    {
                        CreateDataGameObjectCommand.Process(DataScene, it, DataScene);
                        RuntimeEditorController.SelectionBehaviour.AddToSelection(it);
                    }
                }
            }

            if (RuntimeEditorBehaviour.IsInputOverWorld == false)
                return;
            if (RuntimeEditorController.SelectionBehaviour.IsTransformHandleDragging)
                return;
            
            var raycastedWorldObject = _worldRaycasterBehaviour.Raycast();

            // refresh outline
            if (_raycastHitColliderHandler != raycastedWorldObject)
            {
                if (_raycastHitColliderHandler != null)
                    if (_raycastHitColliderHandler.SelectionState == EditorRaycastHitColliderHandlerBehaviour.SelectionStateEnum.Temporary)
                        _raycastHitColliderHandler.SelectionState = EditorRaycastHitColliderHandlerBehaviour.SelectionStateEnum.None;

                _raycastHitColliderHandler = raycastedWorldObject;

                if (_raycastHitColliderHandler != null)
                    if (_raycastHitColliderHandler.SelectionState == EditorRaycastHitColliderHandlerBehaviour.SelectionStateEnum.None)
                        _raycastHitColliderHandler.SelectionState = EditorRaycastHitColliderHandlerBehaviour.SelectionStateEnum.Temporary;
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (_raycastHitColliderHandler != null)
                {
                    var dataGameObjectBehaviour = _raycastHitColliderHandler.GetComponent<DataGameObjectBehaviour>();
                    RuntimeEditorController.SelectionBehaviour.AddToSelection(dataGameObjectBehaviour.DataGameObject);
                }
                else
                {
                    RuntimeEditorController.SelectionBehaviour.ClearSelection();
                }
            }
        }
    }
}


