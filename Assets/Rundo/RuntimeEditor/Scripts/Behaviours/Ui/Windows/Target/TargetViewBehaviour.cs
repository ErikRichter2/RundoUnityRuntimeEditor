using System.Collections.Generic;
using Rundo.RuntimeEditor.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    public class TargetViewBehaviour : EditorBaseBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private TargetViewItemBehaviour _targetViewItemPrefab;
        [SerializeField] private Button _closeBtn;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private DataGameObjectsSearchFilterBehaviour _dataGameObjectsSearchFilterBehaviour;

        private List<TargetViewItemBehaviour> _items = new List<TargetViewItemBehaviour>();
        
        private void Start()
        {
            gameObject.SetActive(false);
            
            _closeBtn.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
            });
        }

        public void SetData(IObjectPickerBehaviour objectPickerBehaviour)
        {
            _title.text = objectPickerBehaviour.Label;
            
            foreach (var it in _items)
                Destroy(it.gameObject);
            
            _items.Clear();

            var items = new List<DataGameObjectTreeHierarchy>();
            
            foreach (var metadata in DataScene.GetTreeHierarchy())
            {
                if (objectPickerBehaviour.GetReferenceType() == typeof(DataGameObject) ||
                    metadata.DataGameObject.HasComponentOfType(objectPickerBehaviour.GetReferenceType()))
                {
                    items.Add(metadata);
                }
            }

            _dataGameObjectsSearchFilterBehaviour.SetData(items, searchResult =>
            {
                foreach (var it in _items)
                    Destroy(it.gameObject);
            
                _items.Clear();

                foreach (var metadata in searchResult)
                {
                    var item = Instantiate(_targetViewItemPrefab, _content);
                    _items.Add(item);
                    item.SetData(objectPickerBehaviour, metadata.DataGameObject);
                }
            });
        }
    }
}
