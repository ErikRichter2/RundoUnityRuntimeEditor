using System;
using Newtonsoft.Json;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data;
using Rundo.Ui;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    [DataComponent]
    [DataTypeId("4b67658b-a4a7-4629-8040-6ab1369be003")]
    public class DataTransformBehaviour : DataComponentMonoBehaviour
    {
        private Transform _transform;
        private Vector3 _localPosition;
        private Vector3 _localEulerAngles;
        private Vector3 _localScale = Vector3.one;
        
        private void Start()
        {
            _transform = GetComponent<Transform>(); 
            LocalPosition = LocalPosition;
            LocalEulerAngles = LocalEulerAngles;
            LocalScale = LocalScale;
        }

        [JsonIgnore]
        public Vector3 Position
        {
            get
            {
                var pos = LocalPosition;
                
                if (DataGameObject.GetDataGameObjectParent() is DataGameObject dataGameObject)
                    pos += dataGameObject.GetComponent<DataTransformBehaviour>().Data.Position;

                return pos;
            }
            set
            {
                if (DataGameObject.GetDataGameObjectParent() is DataGameObject dataGameObject)
                {
                    var parentPosition = dataGameObject.GetComponent<DataTransformBehaviour>().Data.Position;
                    LocalPosition = value - parentPosition;
                }
                else
                {
                    LocalPosition = value;
                }
            }
        }

        public Vector3 LocalPosition
        {
            get => _localPosition;
            set
            {
                _localPosition = value;
                _localPosition.x = (float)Math.Round(_localPosition.x, 4);
                _localPosition.y = (float)Math.Round(_localPosition.y, 4);
                _localPosition.z = (float)Math.Round(_localPosition.z, 4);
                if (_transform != null)
                    _transform.localPosition = value;
            }
        }

        public Vector3 LocalEulerAngles
        {
            get => _localEulerAngles;
            set
            {
                _localEulerAngles = value;
                if (_transform != null)
                    _transform.localEulerAngles = value;
            }
        }

        public Vector3 LocalScale
        {
            get => _localScale;
            set
            {
                _localScale = value;
                if (_transform != null)
                    _transform.localScale = value;
            }
        }

        public override void OnFromBehaviourToData(DataComponent dataComponent)
        {
            var dataTransform = dataComponent.GetData() as DataTransformBehaviour;
            dataTransform.LocalEulerAngles = transform.eulerAngles;
            dataTransform.LocalScale = transform.localScale;
            dataTransform.LocalPosition = transform.localPosition;
        }
    }
}



