﻿using System.Collections.Generic;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Behaviours.UI;
using Rundo.RuntimeEditor.Commands;
using Rundo.RuntimeEditor.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class HierarchyWindowBehaviour : EditorBaseBehaviour, IMouseEventsListener
    {
        [SerializeField] private Transform _content;
        [SerializeField] private HierarchyWindowItemDataBehaviour _hierarchyViewItemDataPrefab;
        [SerializeField] private HierarchyWindowItemSeparatorBehaviour _hierarchyViewItemSeparatorPrefab;
        [SerializeField] private DataGameObjectsSearchFilterBehaviour _dataGameObjectsSearchFilterBehaviour;
        [SerializeField] private Button _closeBtn;

        private List<HierarchyWindowItemBaseBehaviour> _items = new List<HierarchyWindowItemBaseBehaviour>();

        private bool _invalidateRefresh;
        private HierarchyWindowItemBaseBehaviour _rayHitItem;
        private DragDropBehaviour _dragDropBehaviour;
        
        private void Start()
        {
            RegisterUiEvent<SelectionBehaviour.SelectObjectEvent>(OnSelectionChanged);
            RegisterUiEvent<SelectionBehaviour.UnselectObjectEvent>(OnSelectionChanged);
            RegisterCommandListener<CreateDataGameObjectCommand>(Refresh);
            RegisterCommandListener<DestroyDataGameObjectCommand>(Refresh);
            RegisterCommandListener<SetDataGameObjectParentCommand>(Refresh);
            RegisterCommandListener<DataGameObjectBehaviour>(Refresh);
            
            RegisterUiEvent<EditorUiBehaviour.SetHierarchyExpandedStateEvent>(Refresh);
            RegisterUiEvent<RuntimeEditorSceneControllerBehaviour.OnSceneLoadedEvent>(Refresh);
            
            _invalidateRefresh = true;
            
            _closeBtn.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                DispatchUiEvent(new EditorUiBehaviour.HideWindowEvent{Window = GetType()});
            });
        }

        private void Update()
        {
            if (_invalidateRefresh)
            {
                _invalidateRefresh = false;
                Refresh();
            }

            if (_dragDropBehaviour != null)
            {
                if (_rayHitItem != null)
                {
                    _rayHitItem.SetDragSelection(false);
                    _rayHitItem = null;
                }

                if (_dragDropBehaviour.CurrentHitTarget is HierarchyWindowItemBaseBehaviour
                    hierarchyViewItemBaseBehaviour)
                {
                    _rayHitItem = hierarchyViewItemBaseBehaviour;
                    _rayHitItem.SetDragSelection(true);
                }
            }
            else
            {
                if (_rayHitItem != null)
                {
                    _rayHitItem.SetDragSelection(false);
                    _rayHitItem = null;
                }
            }
        }

        public void StartDrag(HierarchyWindowItemDataBehaviour hierarchyViewItemDataBehaviour)
        {
            if (_dragDropBehaviour != null)
                Destroy(_dragDropBehaviour.gameObject);
            
            var hierarchyItem = Instantiate(_hierarchyViewItemDataPrefab, GetComponentInParent<Canvas>().transform);
            hierarchyItem.SetReadOnly();
            hierarchyItem.GetComponent<RectTransform>().pivot = 0.5f * Vector2.one;
            hierarchyItem.GetComponent<RectTransform>().anchorMin = 0.5f * Vector2.one;
            hierarchyItem.GetComponent<RectTransform>().anchorMax = 0.5f * Vector2.one;
            hierarchyItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            hierarchyItem.SetData(hierarchyViewItemDataBehaviour.DataGameObject, 0);
            _dragDropBehaviour = hierarchyItem.gameObject.AddComponent<DragDropBehaviour>();
            _dragDropBehaviour.SetData(hierarchyViewItemDataBehaviour.DataGameObject);
        }

        private void Refresh()
        {
            foreach (var it in _items)
                Destroy(it.gameObject);
            
            _items.Clear();

            if (RuntimeEditorController.IsSceneLoaded == false)
                return;
            
            _dataGameObjectsSearchFilterBehaviour.SetData(DataScene.GetTreeHierarchy(), Draw);
            
            OnSelectionChanged();
        }

        private void Draw(List<DataGameObjectTreeHierarchy> dataGameObjects)
        {
            foreach (var it in _items)
                Destroy(it.gameObject);
            
            _items.Clear();

            foreach (var metadata in dataGameObjects)
            {
                var isExpression = _dataGameObjectsSearchFilterBehaviour.IsExpression;
                
                if (isExpression == false && metadata.IsHidden)
                    continue;

                var depth = isExpression ? 0 : metadata.Depth;
                
                var item = Instantiate(_hierarchyViewItemDataPrefab, _content);
                _items.Add(item);
                item.SetData(metadata.DataGameObject, depth);
                if (_dataGameObjectsSearchFilterBehaviour.IsExpression)
                    item.HideExpandedCollapsedButtons();

                var separator = Instantiate(_hierarchyViewItemSeparatorPrefab, _content);
                _items.Add(separator);
                separator.SetData(metadata.DataGameObject, depth);
            }
        }

        private void OnSelectionChanged()
        {
            foreach (var it in _items)
                if (it is HierarchyWindowItemDataBehaviour hierarchyViewItemDataBehaviour)
                    hierarchyViewItemDataBehaviour.SetManualSelection(RuntimeEditorController.SelectionBehaviour.IsSelected(it.DataGameObject));
        }

        public void OnClick()
        {
            //throw new NotImplementedException();
        }

        public void OnRightClick()
        {
            var prefab = Resources.Load<ContextMenuBehaviour>("Rundo/Ui/ContextMenu/ContextMenuPrefab");
            var instance = Instantiate(prefab, GetComponentInParent<Canvas>().transform);
            instance
                .AddItemData(new ContextMenuItemData<object>
                {
                    Name = "Add Empty GO",
                    Callback = obj =>
                    {
                        CreateDataGameObjectCommand.Process(DataScene, DataGameObject.Instantiate(), DataScene);
                    }
                })
                .AddItemData(new ContextMenuItemData<object>
                {
                    Name = "Add Lightning",
                    Callback = obj =>
                    {
                        var lightningPrefab =
                            RuntimeEditor.GetPrefab(TGuid<TPrefabId>.Create("cb77c5d7-d145-4122-9df4-0b1e506652f8"));
                        var levelData = DataScene.InstantiateDataGameObjectFromPrefab(lightningPrefab);
                        CreateDataGameObjectCommand.Process(DataScene, levelData, DataScene);
                    }
                });
        }
    }


}
