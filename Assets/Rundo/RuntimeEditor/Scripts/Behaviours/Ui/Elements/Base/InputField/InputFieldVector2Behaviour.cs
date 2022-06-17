using Rundo.Core.Data;
using Rundo.RuntimeEditor.Attributes;
using Rundo.Ui;
using TMPro;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    [CustomInspector(typeof(Vector2), false)]
    public class InputFieldVector2Behaviour : DataBaseBehaviour, IInspectorBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private InputFieldFloatBehaviour _x;
        [SerializeField] private InputFieldFloatBehaviour _y;

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
            UiDataMapper.Map(_x).Bind(nameof(Vector2.x));
            UiDataMapper.Map(_y).Bind(nameof(Vector2.y));
        }
    }
}


