using System.Collections.Generic;
using Rundo.Core.Utils;
using Rundo.RuntimeEditor.Tools;
using Rundo.Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    public class ContextMenuBehaviour : MonoBehaviour
    {
        [SerializeField] private Button _blocker;
        [SerializeField] private Transform _itemsContent;
        [SerializeField] private RectTransform _itemsOffset;
        [SerializeField] private ContextMenuItemBehaviour _itemPrefab;
        
        private readonly List<IContextMenuItemData> _instantiatedItems = new List<IContextMenuItemData>();
        private bool _isKeepOpenWhenHoldingCtrl;
        
        private void Start()
        {
            _blocker.onClick.AddListener(() => Destroy(gameObject));

            GetComponent<CanvasRebuilderBehaviour>().Rebuild(() =>
            {
                RectTransformUtils.SnapToCursor(_itemsOffset, _itemsOffset);
            });
        }
        
        public ContextMenuBehaviour AddItemData<T>(ContextMenuItemData<T> itemData)
        {
            Instantiate(_itemPrefab, _itemsContent).SetData(itemData);
            _instantiatedItems.Add(itemData);
            return this;
        }

        public void SortByName()
        {
            _instantiatedItems.Sort((item1, item2) => string.CompareOrdinal(item1.Name, item2.Name));
            for (int i = 0; i < _instantiatedItems.Count; ++i)
                _instantiatedItems[i].GameObject.transform.SetSiblingIndex(i);
        }

        public void KeepOpenWhenHoldingCtrl()
        {
            _isKeepOpenWhenHoldingCtrl = true;
        }

        public bool CanCloseOnClick {
            get
            {
                if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && _isKeepOpenWhenHoldingCtrl)
                {
                    foreach (var it in _instantiatedItems)
                        if (it.GameObject.GetComponent<ContextMenuItemBehaviour>()?.IsMouseOver ?? false)
                            return false;
                }

                return true;
            }
        }

    }
}

