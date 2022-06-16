using System;
using Rundo.RuntimeEditor.Tools;
using Rundo.Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class ExpandCollapseButtonBehaviour : MonoBehaviour
    {
        [SerializeField] private Button _btn;
        [SerializeField] private GameObject _iconExpanded;
        [SerializeField] private GameObject _iconCollapsed;
        [SerializeField] private Transform _content;

        public bool IsExpanded { get; private set; } = true;

        private Action _onClick;
        
        private void Start()
        {
            _btn.onClick.AddListener(() =>
            {
                IsExpanded = !IsExpanded;
                Redraw();
                _onClick?.Invoke();
            });
            
            Redraw();
        }

        private void Redraw()
        {
            _iconCollapsed.SetActive(!IsExpanded);
            _iconExpanded.SetActive(IsExpanded);

            if (_content != null)
            {
                _content.gameObject.SetActive(IsExpanded);
                GetComponentInParent<CanvasRebuilderBehaviour>()?.Rebuild();
            }
        }

        public void Expand()
        {
            IsExpanded = true;
            Redraw();
        }

        public void Collapse()
        {
            IsExpanded = false;
            Redraw();
        }

        public void OnClick(Action onClick)
        {
            _onClick = onClick;
        }

    }
}


