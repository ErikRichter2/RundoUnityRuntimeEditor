using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class ButtonBehaviour : UiDataMapperButtonElementBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private GameObject _selectedBg;
        
        public bool IsSelected { get; private set; }

        public string Label
        {
            get => _label.text;
            set
            {
                _label.text = value;
                _label.gameObject.SetActive(!string.IsNullOrEmpty(value));
            }
        }

        public void Select()
        {
            IsSelected = true;
            Redraw();
        }

        public void Unselect()
        {
            IsSelected = false;
            Redraw();
        }

        private void Redraw()
        {
            _selectedBg.SetActive(IsSelected);
        }

    }
}


