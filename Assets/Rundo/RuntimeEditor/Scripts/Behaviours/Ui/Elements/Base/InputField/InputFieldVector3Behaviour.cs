using Rundo.Core.Data;
using Rundo.RuntimeEditor.Attributes;
using TMPro;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    [CustomInspector(typeof(Vector3))]
    public class InputFieldVector3Behaviour : DataBaseBehaviour, ICssElement, IInspectorWindowElementBehaviour
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
            
            UiDataMapper.Bind(_x, nameof(Vector3.x));
            UiDataMapper.Bind(_y, nameof(Vector3.y));
            UiDataMapper.Bind(_z, nameof(Vector3.z));
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


