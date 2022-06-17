namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// Base behaviour class for editor specific behaviours - provides a shortcut to a runtime editor context.
    /// </summary>
    public class EditorBaseBehaviour : BaseBehaviour
    {
        private RuntimeEditorBehaviour _runtimeEditor;
    
        protected RuntimeEditorBehaviour RuntimeEditor
        {
            get
            {
                if (_runtimeEditor == null)
                    _runtimeEditor = GetComponentInParent<RuntimeEditorBehaviour>();
                return _runtimeEditor;
            }
        }

        
        private RuntimeEditorSceneControllerBehaviour _runtimeEditorController;

        protected RuntimeEditorSceneControllerBehaviour RuntimeEditorController
        {
            get
            {
                if (_runtimeEditorController == null)
                    _runtimeEditorController = GetComponentInParent<RuntimeEditorSceneControllerBehaviour>();
                return _runtimeEditorController;
            }
        }
    }
}


