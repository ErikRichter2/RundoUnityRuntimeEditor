using System;
using Rundo.RuntimeEditor.Data;
using Rundo.RuntimeEditor.Data.UiDataMapper;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    public class ObjectPickerBehaviour : 
        UiDataMapperElementBehaviour<IDataComponentReference>, 
        IDragDropHandler, 
        IObjectPickerBehaviour,
        ICustomUiDataMapper
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private InputFieldBehaviour _object;
        [SerializeField] private Button _picker;
        [SerializeField] private Button _input;

        private Type _referenceType;
        private Action<UiDataMapperElementValue<IDataComponentReference>> _onSubmit;

        public string Label
        {
            get => _label.text;
            set => _label.text = value;
        }

        private void Start()
        {
            _picker.onClick.AddListener(() =>
            {
                DispatchUiEvent(new EditorUiBehaviour.ShowTargetWindowEvent
                {
                    ObjectPickerBehaviour = this
                });
            });

            _input.onClick.AddListener(() =>
            {
                var go = Value.GetDataGameObject(DataScene);
                if (go != null)
                {
                    RuntimeEditorController.SelectionBehaviour.ClearSelection();
                    RuntimeEditorController.SelectionBehaviour.AddToSelection(go);
                }
            });
        }
        
        public Type GetDataMapperType()
        {
            return typeof(UiDataMapperElementInstance<>).MakeGenericType(new Type[] { typeof(IDataComponentReference) });
        }

        protected override void SetUndefinedValue()
        {
            _object.IsUndefinedValue = true;
        }

        public override void OnSubmit(Action<UiDataMapperElementValue<IDataComponentReference>> onSubmit)
        {
            _onSubmit = onSubmit;
        }

        protected override void SetValueInternal(IDataComponentReference value)
        {
            string goName = value.GetDataGameObject(DataScene)?.Name;
            if (string.IsNullOrEmpty(goName))
                goName = "(none)";
            _object.Value = goName;
        }

        public void SetReferenceType(Type type)
        {
            _referenceType = type;
        }

        public Type GetReferenceType()
        {
            return _referenceType;
        }

        public bool CanHandleDragDrop(DragDropBehaviour dropBehaviour)
        {
            if (dropBehaviour.Data is DataGameObject dataGameObject)
            {
                if (dataGameObject.HasComponentOfType(_referenceType))
                {
                    return true;
                }
            }

            return false;
        }

        public void HandleDragDrop(DragDropBehaviour dropBehaviour)
        {
            if (CanHandleDragDrop(dropBehaviour))
            {
                var dataGameObject = dropBehaviour.Data as DataGameObject;
                Assert.IsNotNull(dataGameObject);
                SubmitValue(dataGameObject);
            }
        }

        public void SubmitValue(DataGameObject dataGameObject)
        {
            var type = typeof(TDataComponentReference<>).MakeGenericType(_referenceType);
            var value = (IDataComponentReference)RundoEngine.DataFactory.Instantiate(type);
            if (dataGameObject != null)
                value.SetDataGameObjectId(dataGameObject.ObjectId);
            _onSubmit?.Invoke(new UiDataMapperElementValue<IDataComponentReference>(value));
        }
        
        public override void InitDefaultCssValues(CssBehaviour cssBehaviour)
        {
            cssBehaviour.SetDefaultValue(CssPropertyEnum.LabelWidth,
                _label.rectTransform.sizeDelta.x);
        }

        public override void UpdateCss(CssBehaviour cssBehaviour)
        {
            if (cssBehaviour.TryGetFloat(CssPropertyEnum.LabelWidth, out var value))
                _label.rectTransform.sizeDelta =
                    new Vector2(value, _label.rectTransform.sizeDelta.y);
        }
    }
}


