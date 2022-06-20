using System;
using System.Collections;
using System.Collections.Generic;
using Rundo.Core.Commands;
using Rundo.Core.Utils;
using Rundo.RuntimeEditor.Factory;
using Rundo.RuntimeEditor.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    /// <summary>
    /// Default inspector for drawing object of IList type
    /// </summary>
    public class DefaultDataListInspectorBehaviour : InspectorWindowElementBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Button _add;
        [SerializeField] private Button _remove;
        [SerializeField] private Transform _content;
        [SerializeField] private DefaultDataListItemInspectorBehaviour _defaultDataListItemInspectorPrefab;
        
        private int _currentListInstanceCnt;
        private IList _currentListInstance;
        private Type _currentType;
        
        private readonly List<DefaultDataListItemInspectorBehaviour> _instantiatedListItems = new List<DefaultDataListItemInspectorBehaviour>();

        protected override Transform GetUiDataMapperDefaultContent => _content;

        public override string Label
        {
            get => _label.text;
            set => _label.text = value;
        }

        protected override void MapUi()
        {
            _add.onClick.AddListener(OnAddItem);
            _remove.onClick.AddListener(OnRemoveItem);
            
            RegisterCommandListener<RemoveFromListCommandEvent>(OnRemoveFromListCommandEvent);
        }

        private void OnRemoveFromListCommandEvent(RemoveFromListCommandEvent data)
        {
            if (GetData().Contains(data.Data))
                Rebuild();
        }

        private void OnRemoveItem()
        {
            var indexesToRemove = new List<int>();
            for (var i = 0; i < _instantiatedListItems.Count; ++i)
                if (_instantiatedListItems[i].IsSelected)
                    indexesToRemove.Add(i);

            if (indexesToRemove.Count <= 0)
                return;

            for (var i = 0; i < UiDataMapper.DataHandler.GetRootData().Count; ++i)
            {
                var list = UiDataMapper.DataHandler.GetRawValue<IList>(i);
                var command = new RemoveFromListCommand(list, indexesToRemove);
                command.AddDispatcherData(new RemoveFromListCommandEvent(UiDataMapper.DataHandler.GetRootData()[0]));
                UiDataMapper.DataHandler.CommandProcessor.Process(command);
            }
            
        }

        private void OnAddItem()
        {
            for (var i = 0; i < UiDataMapper.DataHandler.GetRootData().Count; ++i)
            {
                var list = UiDataMapper.DataHandler.GetRawValue<IList>(i);
                var type = ReflectionUtils.GetListType(list.GetType());
                var command = new AddToListCommand(list, list.Count, RundoEngine.DataSerializer.CreateInstance(type));
                command.AddDispatcherData(new RemoveFromListCommandEvent(UiDataMapper.DataHandler.GetRootData()[0]));
                UiDataMapper.DataHandler.CommandProcessor.Process(command);
            }
        }
        
        protected override void OnDataSetInternal()
        {
            base.OnDataSetInternal();
            Rebuild();
        }

        private void ClearElements()
        {
            foreach (var it in _instantiatedListItems)
                Destroy(it.gameObject);
            
            _instantiatedListItems.Clear();

            UiDataMapper.ClearElements();
        }

        private void Rebuild()
        {
            if (UiDataMapper.DataHandler.GetRootData().Count != 1)
            {
                ClearElements();
                gameObject.SetActive(false);
                return;
            }

            var listInstance = UiDataMapper.DataHandler.GetRawValue<IList>(0);
            var dataType = listInstance.GetType().GetGenericArguments()[0];

            var cnt = listInstance.Count;

            if (gameObject.activeSelf && _currentType != null && _currentListInstanceCnt == cnt && _currentType == dataType)
            {
                Redraw();
                return;
            }

            gameObject.SetActive(true);

            _currentType = dataType;
            _currentListInstance = listInstance;
            _currentListInstanceCnt = cnt;

            ClearElements();
            for (var i = 0; i < _currentListInstance.Count; ++i)
                ProcessListElement(i);

            Redraw();
            
            GetComponentInParent<CanvasRebuilderBehaviour>()?.Rebuild();
        }

        private void OnListItemBehaviourClick(int index)
        {
            _instantiatedListItems[index].IsSelected = !_instantiatedListItems[index].IsSelected;
            _instantiatedListItems[index].SelectionBg.SetActive(_instantiatedListItems[index].IsSelected);
        }

        private void ProcessListElement(int index)
        {
            var itemContent = Instantiate(_defaultDataListItemInspectorPrefab, _content);
            _instantiatedListItems.Add(itemContent);
            
            UiDataMapper.SetUiElementsContent(itemContent.Content);
            
            itemContent.SetData(index);
            itemContent.SetOnClick(OnListItemBehaviourClick);

            // handle primitives
            var uiDataMapperElement = UiDataMapper.CreatePrimitive(_currentType, "", "");
            if (uiDataMapperElement != null)
            {
                uiDataMapperElement.BindListIndex(index);
                return;
            }
            
            var dataHandler = UiDataMapper.DataHandler.Copy().AddListElement(index);
            UiFactory.DrawInspector(dataHandler, itemContent.Content, false);
        }
    }
}
