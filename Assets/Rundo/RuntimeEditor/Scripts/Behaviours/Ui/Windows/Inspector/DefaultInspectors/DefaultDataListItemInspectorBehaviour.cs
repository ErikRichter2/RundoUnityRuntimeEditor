using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DefaultDataListItemInspectorBehaviour : MonoBehaviour
{
    [SerializeField] private Button _select;
    [SerializeField] private Transform _content;
    [SerializeField] private TMP_Text _elementName;
    [SerializeField] private GameObject _selectionBg;

    public Transform Content => _content;
    public GameObject SelectionBg => _selectionBg;

    private int _index;
    private Action<int> _onClick;
    
    public bool IsSelected { get; set; }

    private void Start()
    {
        _select.onClick.AddListener(() =>
        {
            _onClick?.Invoke(_index);
        });
    }

    public void SetData(int index)
    {
        _index = index;
        _elementName.text = $"Element {_index}";
    }

    public void SetOnClick(Action<int> onClick)
    {
        _onClick = onClick;
    }
}
