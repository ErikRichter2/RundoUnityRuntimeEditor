using Rundo.Core.Commands;
using Rundo.RuntimeEditor.Commands;
using Rundo.RuntimeEditor.Data;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// Used for placing prefabs over the zero Y-plane
    /// </summary>
    public class PlaceObjectsEditorModeBehaviour : EditorModeBaseBehaviour
    {
        private EditorRaycastHitColliderHandlerBehaviour _raycastHitColliderHandler;
        private DataGameObject _dataGameObject;
        private GameObject _gameObject;
        private Plane _plane;

        private void Start()
        {
            _plane = new Plane(Vector3.up, Vector3.zero);
        }

        protected override void OnDestroyInternal()
        {
            base.OnDestroyInternal();
            RuntimeEditorController.SelectionBehaviour.ClearSelection();
            Destroy(_gameObject);
        }

        private void Update()
        {
            // cancel mode
            if (Input.GetMouseButtonDown(1))
            {
                RuntimeEditorController.SetMode<SelectObjectsEditorModeBehaviour>();
                return;
            }
            
            // mouse raycast
            if (_dataGameObject != null)
            {
                var ray = RuntimeEditorController.ActiveCamera.ScreenPointToRay(Input.mousePosition);
                if (_plane.Raycast(ray, out var distance))
                {
                    var hitPoint = ray.GetPoint(distance);
                    var worldPos = new Vector3(hitPoint.x, 0, hitPoint.z);

                    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    {
                        worldPos.x = Mathf.RoundToInt(worldPos.x);
                        worldPos.z = Mathf.RoundToInt(worldPos.z);
                    }

                    // sync datamodel with cursor position
                    var dataTransformBehaviour = _dataGameObject.GetComponent<DataTransformBehaviour>();
                    var command = new SetValueToMemberCommand(dataTransformBehaviour.Data, nameof(DataTransformBehaviour.LocalPosition), worldPos);
                    command.SetIgnoreUndoRedo();
                    command.AddDispatcherData(dataTransformBehaviour);
                    DataScene.CommandProcessor.Process(command);
                }

                // place
                if (Input.GetMouseButtonDown(0))
                {
                    CreateDataGameObjectCommand.Process(DataScene, RundoEngine.DataSerializer.Clone(_dataGameObject), DataScene);
                }
            }
        }

        public async void SetData(DataGameObject dataGameObject)
        {
            RuntimeEditorController.SelectionBehaviour.ClearSelection();
            
            _dataGameObject = dataGameObject;
            
            _gameObject = await DataScene.InstantiateGameObject(BaseDataProvider, _dataGameObject, null);
            _gameObject.transform.SetParent(transform, true);
            
            RuntimeEditorController.SelectionBehaviour.AddToSelectionWithoutTransformGizmo(_dataGameObject);
        }
    }
}


