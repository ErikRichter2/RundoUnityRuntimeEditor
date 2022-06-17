using Rundo.RuntimeEditor.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    public class TargetViewItemBehaviour : EditorBaseBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Button _btn;
        [SerializeField] private GameObject _selection;

        private DataGameObject _dataGameObject;
        private IObjectPickerBehaviour _objectPickerBehaviour;
        private static TargetViewItemBehaviour _selected;

        private void Start()
        {
            _selection.SetActive(false);

            var go = _objectPickerBehaviour.Value.GetDataGameObject(DataScene);
            if (_dataGameObject == go)
                Select();
            
            _selection.SetActive(_dataGameObject == _objectPickerBehaviour.Value.GetDataGameObject(DataScene));
            _btn.onClick.AddListener(() =>
            {
                Select();
                _objectPickerBehaviour.SubmitValue(_dataGameObject);
            });
        }

        private void Select()
        {
            if (_selected != null)
                _selected._selection.SetActive(false);

            _selection.SetActive(true);
            _selected = this;
        }

        public void SetData(IObjectPickerBehaviour objectPickerBehaviour, DataGameObject dataGameObject)
        {
            _objectPickerBehaviour = objectPickerBehaviour;
            _dataGameObject = dataGameObject;
            _icon.gameObject.SetActive(false);

            if (_dataGameObject != null)
                _text.text = _dataGameObject.Name;
            else
                _text.text = "(none)";
        }
    }
}
