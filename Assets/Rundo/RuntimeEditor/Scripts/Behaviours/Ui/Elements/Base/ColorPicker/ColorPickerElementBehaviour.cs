using System;
using Rundo.Core.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class ColorPickerElementBehaviour : MonoBehaviour
    {
        [SerializeField] private Image _colorImg;
        [SerializeField] private Image _colorImgWithAlpha;
        [SerializeField] private Button _selectColorBtn;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private ColorPickerChooseColorBehaviour _colorPickerChooseColor;
        [SerializeField] private RectTransform _content;

        private GameObject _blocker;
        
        public Action<Color> OnColorChanged;
        public Action<Color> OnColorConfirmed;

        public TMP_Text LabelElement => _label;

        public string Label
        {
            get => _label.text;
            set => _label.text = value;
        }

        private void Start()
        {
            _content.gameObject.SetActive(false);
            
            _colorPickerChooseColor.OnColorChanged(OnColorChangedInternal);
            _colorPickerChooseColor.OnColorConfirmed(OnColorConfirmedInternal);
            
            _selectColorBtn.onClick.AddListener(() =>
            {
                _colorPickerChooseColor.SetInitialColor();
                _content.gameObject.SetActive(true);
                RectTransformUtils.FlipRectTransformIfOutsideOfScreen(_content);
                Destroy(_blocker);
                _blocker = RectTransformUtils.CreateUIBlocker(
                    _content.GetComponentInParent<Canvas>().rootCanvas, 
                    _content.GetComponentInParent<Canvas>(), 
                    transform, _colorPickerChooseColor.Confirm);
            });

            Redraw();
        }

        private void Redraw()
        {
            var c = _colorPickerChooseColor.CurrentColor;
            _colorImgWithAlpha.color = c;
            c.a = 1f;
            _colorImg.color = c;
        }

        private void OnColorChangedInternal(Color color)
        {
            Redraw();
            OnColorChanged?.Invoke(color);
        }
        
        private void OnColorConfirmedInternal(Color color)
        {
            _content.gameObject.SetActive(false);
            Destroy(_blocker);
            _blocker = null;
            Redraw();
            OnColorConfirmed?.Invoke(color);
        }

        public void SetColor(Color color)
        {
            _colorPickerChooseColor.SetColor(color);
            Redraw();
        }

        public Color GetColor()
        {
            return _colorPickerChooseColor.CurrentColor;
        }
    }
}
