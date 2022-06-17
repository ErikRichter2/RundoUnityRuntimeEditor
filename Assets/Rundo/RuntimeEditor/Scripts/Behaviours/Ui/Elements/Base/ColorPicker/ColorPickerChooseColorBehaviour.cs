using System;
using HSVPicker;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class ColorPickerChooseColorBehaviour : MonoBehaviour
    {
        [SerializeField] private ColorPicker _colorPicker;
        [SerializeField] private Button _confirm;
        [SerializeField] private Button _cancel;

        private Action<Color> _onColorChanged;
        private Action<Color> _onColorConfirmed;
        
        private Color _prevColor;
        private Color _initialColor;

        public Color CurrentColor
        {
            get => _colorPicker.CurrentColor;
            set => _colorPicker.CurrentColor = value;
        }

        private void Start()
        {
            _confirm.onClick.AddListener(Confirm);
            _cancel.onClick.AddListener(OnCancel);
        }

        public void SetInitialColor()
        {
            _initialColor = CurrentColor;
        }

        private void Update()
        {
            if (_prevColor != _colorPicker.CurrentColor)
            {
                _prevColor = _colorPicker.CurrentColor;
                _onColorChanged?.Invoke(_prevColor);
            }
        }

        public void OnColorChanged(Action<Color> onColorChanged)
        {
            _onColorChanged = onColorChanged;
        }

        public void OnColorConfirmed(Action<Color> onColorConfirmed)
        {
            _onColorConfirmed = onColorConfirmed;
        }

        public void SetColor(Color color)
        {
            _colorPicker.CurrentColor = _prevColor = color;
        }

        public void Confirm()
        {
            var color = _colorPicker.CurrentColor;
            _onColorChanged?.Invoke(_initialColor);
            SetColor(color);
            _onColorConfirmed?.Invoke(_colorPicker.CurrentColor);
        }

        private void OnCancel()
        {
            SetColor(_initialColor);
            _onColorChanged?.Invoke(_initialColor);
        }
    }
}

