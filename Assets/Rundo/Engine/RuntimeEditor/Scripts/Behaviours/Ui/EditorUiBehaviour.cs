using System;
using Rundo.RuntimeEditor.Data;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class EditorUiBehaviour : EditorBaseBehaviour
    {
        public struct ToggleWindowEvent : IUiEvent
        {
            public Type Window;
        }

        public struct HideWindowEvent : IUiEvent
        {
            public Type Window;
        }

        public struct ShowTargetWindowEvent : IUiEvent
        {
            public IObjectPickerBehaviour ObjectPickerBehaviour;
        }
        
        public struct SetHierarchyExpandedStateEvent : IUiEvent
        {
            public bool IsExpanded;
            public DataGameObjectId DataGameObjectId;
        }

        public InspectorPopupBehaviour InspectorWindow => GetComponentInChildren<InspectorPopupBehaviour>(true);
        public HierarchyWindowBehaviour HierarchyWindow => GetComponentInChildren<HierarchyWindowBehaviour>(true);
        public PrefabsWindowBehaviour ProjectWindow => GetComponentInChildren<PrefabsWindowBehaviour>(true);

        private void Start()
        {
            RegisterUiEvent<ToggleWindowEvent>(OnToggleWindowEvent);
            RegisterUiEvent<ShowTargetWindowEvent>(OnShowTargetWindowEvent);
            RegisterUiEvent<SetHierarchyExpandedStateEvent>(OnSetHierarchyExpandedStateEvent).SetPriority(999);
            RegisterUiEvent<RuntimeEditorBehaviour.OnSceneSetToTabEvent>(Redraw);
            RegisterUiEvent<RuntimeEditorSceneControllerBehaviour.OnPlayModeChanged>(Redraw);
            Redraw();
        }

        private void Redraw()
        {
            if (RuntimeEditorController.IsEditorMode == false)
            {
                gameObject.SetActive(false);
                return;
            }
            
            gameObject.SetActive(true);
            
            InspectorWindow.gameObject.SetActive(RuntimeEditorController.IsSceneLoaded);
            HierarchyWindow.gameObject.SetActive(RuntimeEditorController.IsSceneLoaded);
            ProjectWindow.gameObject.SetActive(RuntimeEditorController.IsSceneLoaded);
        }

        private void OnSetHierarchyExpandedStateEvent(SetHierarchyExpandedStateEvent data)
        {
            var prefs = RuntimeEditorBehaviour.PersistentEditorPrefs.LoadData();
            
            if (data.IsExpanded)
            {
                if (prefs.ExpandedDataGameObjectsInHierarchyWindow.Contains(data.DataGameObjectId) == false)
                {
                    prefs.ExpandedDataGameObjectsInHierarchyWindow.Add(data.DataGameObjectId);
                    RuntimeEditorBehaviour.PersistentEditorPrefs.SaveData(prefs);
                }
            }
            else
            {
                if (prefs.ExpandedDataGameObjectsInHierarchyWindow.Contains(data.DataGameObjectId))
                {
                    prefs.ExpandedDataGameObjectsInHierarchyWindow.Remove(data.DataGameObjectId);
                    RuntimeEditorBehaviour.PersistentEditorPrefs.SaveData(prefs);
                }
            }
        }

        private void OnToggleWindowEvent(ToggleWindowEvent data)
        {
            var window = GetComponentInChildren(data.Window, true);
            window.gameObject.SetActive(!window.gameObject.activeSelf);
        }

        private void OnShowTargetWindowEvent(ShowTargetWindowEvent data)
        {
            var window = GetComponentInChildren<TargetViewBehaviour>(true);
            window.gameObject.SetActive(true);
            window.SetData(data.ObjectPickerBehaviour);
        }
    }
}


