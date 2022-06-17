using Rundo.Core.Data;
using Rundo.RuntimeEditor.Attributes;
using Rundo.Ui;
using TMPro;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    [CustomInspector(typeof(Vector3), false)]
    public class InputFieldVector3Behaviour : DataBaseBehaviour, ICssElement, IInspectorBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private InputFieldFloatBehaviour _x;
        [SerializeField] private InputFieldFloatBehaviour _y;
        [SerializeField] private InputFieldFloatBehaviour _z;

        public GameObject GameObject => gameObject;

        public string Label
        {
            get => _label.text;
            set => _label.text = value;
        }

        public void SetData(DataHandler dataHandler, string label)
        {
            Label = label;
            SetData(dataHandler);
        }

        protected override void MapUi()
        {
            GetOrCreateCss().SuppressChildrenCss();
            
            UiDataMapper.Map(_x).Bind(nameof(Vector3.x));
            UiDataMapper.Map(_y).Bind(nameof(Vector3.y));
            UiDataMapper.Map(_z).Bind(nameof(Vector3.z));
        }

        public CssBehaviour GetOrCreateCss()
        {
            if (TryGetComponent<CssBehaviour>(out var cssBehaviour))
                return cssBehaviour;
            return gameObject.AddComponent<CssBehaviour>();
        }

        public void InitDefaultCssValues(CssBehaviour cssBehaviour)
        {
            cssBehaviour.SetDefaultValue(CssPropertyEnum.LabelWidth,
                _label.rectTransform.sizeDelta.x);
        }

        public void UpdateCss(CssBehaviour cssBehaviour)
        {
            if (cssBehaviour.TryGetFloat(CssPropertyEnum.LabelWidth, out var value))
                _label.rectTransform.sizeDelta =
                    new Vector2(value, _label.rectTransform.sizeDelta.y);
        }
    }
}


