using Rundo.Core.Commands;
using Rundo.RuntimeEditor.Data;
using RuntimeHandle;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// A helper data when dragging with transform gizmo - to change/commit values changed by dragging transform gizmo.
    /// </summary>
    public class SelectionTransformHandleSync
    {
        private readonly RuntimeTransformHandle _runtimeTransformHandle;
        private Transform _originalParent;
        private Vector3 _originalParentPosition;
        private readonly IDataComponent<DataTransformBehaviour> _dataTransformBehaviour;
        private Vector3 _thisPositionAtDragStart;
        private Vector3 _transformHandlerPositionAtDragStart;
        private readonly Vector3 _startDragHandlePosition;
        private readonly Vector3 _startDragThisLocalPosition;
        private readonly Vector3 _startDragThisPosition;
        private Quaternion _startDragThisLocalRotation;
        private readonly Vector3 _startDragThisLocalScale;
        private GameObject GameObject { get; set; }
        private readonly Transform _transform;

        public SelectionTransformHandleSync(
            RuntimeTransformHandle runtimeTransformHandle,
            GameObject gameObject)
        {
            _runtimeTransformHandle = runtimeTransformHandle;
            GameObject = gameObject;
            _transform = GameObject.transform;
            _dataTransformBehaviour = GameObject.GetComponent<DataGameObjectBehaviour>().DataGameObject.GetComponent<DataTransformBehaviour>();
            _startDragThisLocalPosition = _transform.localPosition;
            _startDragThisLocalRotation = _transform.localRotation;
            _startDragThisPosition = _transform.position;
            _startDragThisLocalScale = _transform.localScale;
            _startDragHandlePosition = _runtimeTransformHandle.target.position;
            _dataTransformBehaviour.SkipUpdateOfRuntimeBehaviour = true;
        }

        public void StopDrag()
        {
            _dataTransformBehaviour.SkipUpdateOfRuntimeBehaviour = false;
            Commit();
        }

        public void ProcessDrag()
        {
            if (_runtimeTransformHandle.type == HandleType.POSITION)
            {
                var changedPosition = _runtimeTransformHandle.target.position - _startDragHandlePosition;
                var nextLocalPosition = _startDragThisLocalPosition + changedPosition;
                    
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    nextLocalPosition.x = Mathf.RoundToInt(nextLocalPosition.x);
                    nextLocalPosition.z = Mathf.RoundToInt(nextLocalPosition.z);
                }

                _transform.localPosition = nextLocalPosition;
            }
            if (_runtimeTransformHandle.type == HandleType.SCALE)
            {
                var scale = new Vector3(
                    _startDragThisLocalScale.x * _runtimeTransformHandle.target.localScale.x,
                    _startDragThisLocalScale.y * _runtimeTransformHandle.target.localScale.y,
                    _startDragThisLocalScale.z * _runtimeTransformHandle.target.localScale.z);
                _transform.localScale = scale;
            }
            else if (_runtimeTransformHandle.type == HandleType.ROTATION)
            {
                var axis = ((RotationAxis)_runtimeTransformHandle.DraggingHandle).Axis;

                Quaternion rot = Quaternion.AngleAxis(_runtimeTransformHandle.DraggingHandle.delta * 180f / Mathf.PI, axis);
                _transform.position = rot * (_startDragThisPosition - _startDragHandlePosition) + _startDragHandlePosition;
                _transform.rotation = rot * _startDragThisLocalRotation;
            }
                
            // sync data transform with behaviour transfrom
            _dataTransformBehaviour.Data.LocalPosition = _transform.localPosition;
            _dataTransformBehaviour.Data.LocalScale = _transform.localScale;
            _dataTransformBehaviour.Data.LocalEulerAngles = _transform.localEulerAngles;

            // dispatch event to force inspector redraw
            _dataTransformBehaviour.DataGameObject.GetCommandProcessor().EventDispatcher.Dispatch(_dataTransformBehaviour, true);
        }

        private void Commit()
        {
            var localPosition = _transform.localPosition;
            var localRotation = _transform.localRotation.eulerAngles;
            var localScale = _transform.localScale;
            
            // set model to original position
            _dataTransformBehaviour.Data.LocalPosition = _startDragThisLocalPosition;
            _dataTransformBehaviour.Data.LocalEulerAngles = _startDragThisLocalRotation.eulerAngles;
            _dataTransformBehaviour.Data.LocalScale = _startDragThisLocalScale;
            
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                localPosition.x = Mathf.RoundToInt(localPosition.x);
                localPosition.z = Mathf.RoundToInt(localPosition.z);
            }

            if (_dataTransformBehaviour.Data.LocalPosition != localPosition)
            {
                var command = new SetValueToMemberCommand(_dataTransformBehaviour.Data, nameof(DataTransformBehaviour.LocalPosition), localPosition);
                command.AddDispatcherData(_dataTransformBehaviour);
                command.Process();
            }

            if (_dataTransformBehaviour.Data.LocalEulerAngles != localRotation)
            {
                var command = new SetValueToMemberCommand(_dataTransformBehaviour.Data, nameof(DataTransformBehaviour.LocalEulerAngles), localRotation);
                command.AddDispatcherData(_dataTransformBehaviour);
                command.Process();
            }

            if (_dataTransformBehaviour.Data.LocalScale != localScale)
            {
                var command = new SetValueToMemberCommand(_dataTransformBehaviour.Data, nameof(DataTransformBehaviour.LocalScale), localScale);
                command.AddDispatcherData(_dataTransformBehaviour);
                command.Process();
            }
        }
    }
}



