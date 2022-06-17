using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// A top menu tabs list.
    /// </summary>
    public class RuntimeEditorTabMenuBehaviour : EditorBaseBehaviour
    {
        [SerializeField] private ButtonBehaviour _addTab;
        [SerializeField] private RuntimeEditorTabButtonBehaviour _tabButton;
        [SerializeField] private Transform _content;

        private void Start()
        {
            _addTab.OnClick(() =>
            {
                RuntimeEditor.AddTab();
            });

            RegisterUiEvent<RuntimeEditorBehaviour.OnTabAddedEvent>(Redraw);
            RegisterUiEvent<RuntimeEditorBehaviour.OnTabRemovedEvent>(Redraw);
            
            Redraw();
        }

        private void Redraw()
        {
            foreach (Transform child in _content)
                Destroy(child.gameObject);

            foreach (var it in RuntimeEditor.InstantiatedTabs)
            {
                var btn = Instantiate(_tabButton, _content);
                btn.SetData(it.TabGuid);
            }
        }
    }
}



