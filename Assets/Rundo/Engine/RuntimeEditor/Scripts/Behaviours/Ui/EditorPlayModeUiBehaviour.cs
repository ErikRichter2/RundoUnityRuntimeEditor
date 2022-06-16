using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class EditorPlayModeUiBehaviour : BaseBehaviour
    {
        [SerializeField] private ButtonBehaviour _editor;

        private void Start()
        {
            _editor.OnClick(() =>
            {
                UiEventsDispatcher.Dispatch(new RuntimeEditorPlayModeControllerBehaviour.OnExitPlayModeEvent());
            });
        }
    }
}


