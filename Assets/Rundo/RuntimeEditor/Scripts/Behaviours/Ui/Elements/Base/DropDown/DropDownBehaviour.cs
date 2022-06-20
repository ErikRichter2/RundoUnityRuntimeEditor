using System;
using System.Collections.Generic;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data.UiDataMapper;
using Rundo.Core.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class DropDownBehaviour : 
        UiDataMapperElementBehaviour<string>,
        IUiDataMapperDynamicValuesCustomHandler,
        ICustomUiDataMapper
    {
        public interface IDropDownOption
        {
            string UniqueId { get; }
            Sprite Sprite { get; set; }
            string Label { get; }
            string OptionsLabel { get; }
        }

        public class DropDownOption : IDropDownOption
        {
            public string UniqueId { get; set; }
            public Sprite Sprite { get; set; }
            public string Label { get; set; }

            private string _optionsLabel;

            public string OptionsLabel
            {
                get => string.IsNullOrEmpty(_optionsLabel) ? Label : _optionsLabel;
                set => _optionsLabel = value;
            }

            public DropDownOption()
            {
                
            }

            public DropDownOption(string id, string label)
            {
                UniqueId = id;
                Label = label;
                OptionsLabel = label;
            }
        }

        private class InstantiatedElement
        {
            public GameObject GameObject;
            public Image Sprite;
            public TextMeshProUGUI Label;
            public Button Button;
            public Image Selected;
        }

        public Action<IDropDownOption, Image> SpriteProviderAsync;
        public Func<IDropDownOption, Sprite> SpriteProvider;
        public Action OnDropDownOpened;

        public bool ReadOnly
        {
            get => _readOnly;
            set
            {
                _readOnly = value;
                RefreshReadOnly();
            }
        }

        public int OptionsCount => _dataProvider.Count;

        [SerializeField] private TMP_Text _label;
        [SerializeField] private ExpressionEvaluatorBehaviour _expressionEvaluator;
        [SerializeField] private GameObject _options;
        [SerializeField] private SimpleScrollViewControllerBehaviour _optionsContent;
        [SerializeField] private Button _optionsBtn;
        [SerializeField] private bool _readOnly;
        [SerializeField] private bool DoNotOverwriteButtonLabel;

        private InstantiatedElement _nullKeyInstantiatedElement;
        private GameObject _blocker;
        private GameObject _optionPrefab;
        private Action<UiDataMapperElementValue<string>> _onSubmit;
        
        private readonly SimpleScrollViewControllerData<IDropDownOption> _dataProvider = new SimpleScrollViewControllerData<IDropDownOption>();
        private readonly Dictionary<string, InstantiatedElement> _instantiatedElements = new Dictionary<string,InstantiatedElement>();

        private InstantiatedElement GetInstantiatedElement(string key)
        {
            if (string.IsNullOrEmpty(key))
                return _nullKeyInstantiatedElement;
            if (_instantiatedElements.TryGetValue(key, out var element))
                return element;
            return null;
        }

        private void SetInstantiatedElement(string key, InstantiatedElement element)
        {
            if (string.IsNullOrEmpty(key))
                _nullKeyInstantiatedElement = element;
            else
                _instantiatedElements[key] = element;
        }

        private void Start()
        {
            if (_optionsContent.transform.childCount > 0)
            {
                var firstChild = _optionsContent.transform.GetChild(0);
                _optionPrefab = Instantiate(firstChild.gameObject, transform);
                _optionPrefab.SetActive(false);
                
                _dataProvider.DataActivated = (index, obj) =>
                {
                    var data = _dataProvider.Get(index);
                    GetInstantiatedElement(data.UniqueId)?.Selected.gameObject.SetActive(Value == data.UniqueId);
                };

                _dataProvider.InstanceProvider = (data, transform, index) =>
                {
                    var element = new InstantiatedElement();

                    SetInstantiatedElement(data.UniqueId, element);
                    
                    element.GameObject = Instantiate(_optionPrefab, transform);
                    
                    var label = element.GameObject.GetComponentInChildren<DropDownLabelElementBehaviour>(true);
                    if (label != null)
                    {
                        element.Label = label.GetComponent<TextMeshProUGUI>();
                        element.Label.text = data.OptionsLabel;
                    }

                    var sprite = element.GameObject.GetComponentInChildren<DropDownSpriteElementBehaviour>(true);
                    if (sprite != null)
                    {
                        if (data.Sprite == null && SpriteProvider == null && SpriteProviderAsync == null)
                        {
                            sprite.gameObject.SetActive(false);
                        }
                        else
                        {
                            sprite.gameObject.SetActive(true);
                            element.Sprite = sprite.GetComponent<Image>();
                            if (data.Sprite == null)
                            {
                                if (SpriteProvider != null)
                                    data.Sprite = SpriteProvider?.Invoke(data);
                                else
                                    SpriteProviderAsync?.Invoke(data, element.Sprite);
                            }
                            element.Sprite.sprite = data.Sprite;
                        }
                    }

                    var selected = element.GameObject.GetComponentInChildren<DropDownSelectedElementBehaviour>(true);
                    if (selected != null)
                    {
                        element.Selected = selected.GetComponent<Image>();
                    }

                    var button = element.GameObject.GetComponentInChildren<DropDownButtonElementBehaviour>(true);
                    if (button != null)
                    {
                        element.Button = button.GetComponent<Button>();
                        element.Button.onClick.AddListener(() =>
                        {
                            OnSelectInternal(data);
                            Hide();
                        });
                    }
                    element.GameObject.SetActive(true);
                    return element.GameObject;
                };
            }

            _options.SetActive(false);
            
            _optionsContent.SetDataProvider(_dataProvider);
            _optionsBtn.onClick.AddListener(() =>
            {
                if (_options.activeSelf)
                {
                    Hide();
                }
                else
                {
                    _options.SetActive(true);
                    OnDropDownOpened?.Invoke();

                    _expressionEvaluator.Expression = "";
                    ApplySearchFilter();
                    _optionsContent.Redraw();
                    for (int i = 0; i < _dataProvider.Count; ++i)
                        if (_dataProvider.Get(i).UniqueId == Value)
                        {
                            _optionsContent.ScrollToIndex(i);
                            break;
                        }
                    
                    RectTransformUtils.FlipRectTransformIfOutsideOfScreen(_options.GetComponent<RectTransform>());

                    _blocker = RectTransformUtils.CreateUIBlocker(_options.GetComponentInParent<Canvas>().rootCanvas, _options.GetComponentInParent<Canvas>(), _optionsContent.transform, Hide);
                }
            });
            
            _expressionEvaluator.OnExpressionChange(ApplySearchFilter);
        }

        protected override void OnDestroyInternal()
        {
            base.OnDestroyInternal();
            Clear();
        }

        public void SetLabel(string label)
        {
            _label.gameObject.SetActive(true);
            _label.text = label;
        }

        private void RefreshReadOnly()
        {
            _optionsBtn.interactable = !_readOnly;
        }

        private void ApplySearchFilter()
        {
            var filter = _expressionEvaluator.Expression?.ToLower();
            for (int i = 0; i < _dataProvider.Count; ++i)
            {
                var data = _dataProvider.Get(i);

                if (string.IsNullOrEmpty(filter))
                    _dataProvider.SetIsInvisibleAtIndex(i, false);
                else
                    _dataProvider.SetIsInvisibleAtIndex(i, !_expressionEvaluator.Evaluate(literal => CanShowItem(literal, data)));
            }

            _optionsContent.Redraw();
        }

        private bool CanShowItem(string literal, IDropDownOption data)
        {
            return data.UniqueId.ToLower().Contains(literal) || data.OptionsLabel.ToLower().Contains(literal);
        }

        private void OnSelectInternal(IDropDownOption data)
        {
            Value = data.UniqueId;
            _onSubmit?.Invoke(new UiDataMapperElementValue<string>(Value));
        }

        private void OnTextSubmitInternal(string value)
        {
            Value = value;
            RefreshSelection();
            _onSubmit?.Invoke(new UiDataMapperElementValue<string>(Value));
        }

        public void Clear()
        {
            Hide();
            _dataProvider?.Clear();
            _nullKeyInstantiatedElement = null;
            foreach (var it in _instantiatedElements.Values)
                Destroy(it.GameObject);
            _instantiatedElements.Clear();
        }
        
        private void RefreshSelection()
        {
            if (_options.activeSelf)
            {
                foreach (var it in _instantiatedElements)
                    it.Value.Selected.gameObject.SetActive(it.Key == Value);
                _nullKeyInstantiatedElement?.Selected.gameObject.SetActive(string.IsNullOrEmpty(Value));
            }
        }

        public void OnValueChanged(Action<IDropDownOption> callback)
        {
            
        }

        public SimpleScrollViewControllerData<IDropDownOption> GetDataProvider()
        {
            return _dataProvider;
        }

        public IDropDownOption GetData(string uniqueId)
        {
            for (int i = 0; i < _dataProvider.Count; ++i)
                if (_dataProvider.Get(i).UniqueId == uniqueId)
                    return _dataProvider.Get(i);
            return null;
        }

        public T GetData<T>(string uniqueId) where T: IDropDownOption
        {
            return (T)GetData(uniqueId);
        }
        
        public void AddData(string uniqueId, string label)
        {
            AddData(new DropDownOption
            {
                UniqueId = uniqueId,
                Label = label,
                OptionsLabel = label,
                Sprite = null,
            });
        }

        public void AddData(string uniqueId, string label, Sprite sprite, string optionsLabel = null)
        {
            AddData(new DropDownOption
            {
                UniqueId = uniqueId,
                Label = label,
                OptionsLabel = string.IsNullOrEmpty(optionsLabel) ? label : optionsLabel,
                Sprite = sprite,
            });
        }

        public int GetDataCount()
        {
            return GetDataProvider().Count;
        }

        public void AddData(IDropDownOption data)
        {
            _dataProvider.Add(data);
            _optionsContent.Rebuild();
        }

        public override void OnSubmit(Action<UiDataMapperElementValue<string>> onSubmit)
        {
            _onSubmit = onSubmit;
        }

        protected override void SetValueInternal(string value)
        {
            if (DoNotOverwriteButtonLabel == false)
                _optionsBtn.GetComponentInChildren<TextMeshProUGUI>().text = GetData(value)?.Label ?? "???";
            RefreshSelection();
        }

        private void Hide()
        {
            if (_blocker != null)
            {
                Destroy(_blocker);
                _blocker = null;
            }

            _options.SetActive(false);
        }

        public IDropDownOption GetCurrentValue()
        {
            if (IsUndefinedValue)
                return null;
            return GetData(Value);
        }

        protected override void SetUndefinedValue()
        {
            _optionsBtn.GetComponentInChildren<TextMeshProUGUI>().text = "--";
        }

        public Type GetDataMapperType()
        {
            return typeof(UiDataMapperElementInstance<>).MakeGenericType(new Type[] { typeof(string) });
        }

        public void SetMouseDragValueChange(bool value)
        {
            //throw new NotImplementedException();
        }

        public void FillFromEnum(Type enumType)
        {
            foreach (var it in EnumUtils.GetEnumValues(enumType))
            {
                AddData(new DropDownOption
                {
                    UniqueId = it.ToString(),
                    Label = it.ToString(),
                    Sprite = null,
                    OptionsLabel = it.ToString(),
                });
            }
        }

        public void FillFromEnum<T>() where T : Enum
        {
            foreach (var it in EnumUtils.GetEnumValues<T>())
            {
                AddData(new DropDownOption
                {
                    UniqueId = it.ToString(),
                    Label = it.ToString(),
                    Sprite = null,
                    OptionsLabel = it.ToString(),
                });
            }
        }

        public override void SetDynamicValue(object dataValue)
        {
            if (dataValue == null)
                Value = null;
            else if (dataValue is string s)
                Value = s;
            else if (dataValue is Enum e)
                Value = e.ToString();
            else if (dataValue is ITEnum iteEnum)
                Value = iteEnum.ToStringRawValue();
            else
                throw new Exception($"Unhandled value type {dataValue.GetType().Name}");
        }

        public override void OnSubmitDynamicValue(Type expectedDataType, Action<UiDataMapperElementValue<object>> onSubmitDynamicValue)
        {
            if (typeof(string).IsAssignableFrom(expectedDataType))
            {
                OnSubmit(value =>
                {
                    onSubmitDynamicValue.Invoke(value.ToDynamic());
                });
            }
            else if (typeof(Enum).IsAssignableFrom(expectedDataType) ||
                     typeof(ITEnum).IsAssignableFrom(expectedDataType))
            {
                OnSubmit(value =>
                {
                    object dynamicValue = default;

                    if (typeof(string).IsAssignableFrom(expectedDataType))
                        dynamicValue = value.Value;
                    else if (typeof(Enum).IsAssignableFrom(expectedDataType) ||
                             typeof(ITEnum).IsAssignableFrom(expectedDataType))
                    {
                        dynamicValue = Enum.Parse(expectedDataType, value.Value);
                    }
                    else
                        throw new Exception($"Unhandled value type {expectedDataType.Name}");
                    
                    onSubmitDynamicValue.Invoke(UiDataMapperElementValue<object>.ToDynamic(dynamicValue, value.MetaData));
                });
            }
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

