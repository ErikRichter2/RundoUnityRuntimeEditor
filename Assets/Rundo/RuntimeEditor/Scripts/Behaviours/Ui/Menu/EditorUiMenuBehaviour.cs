using System.Collections.Generic;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data;
using Rundo.RuntimeEditor.Tools;
using RuntimeHandle;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    public class EditorUiMenuBehaviour : EditorBaseBehaviour
    {
        [SerializeField] private Button _play;
        [SerializeField] private Button _new;
        [SerializeField] private DropDownBehaviour _load;
        [SerializeField] private Button _save;
        [SerializeField] private Button _undo;
        [SerializeField] private Button _redo;
        [SerializeField] private Button _scene;
        [SerializeField] private Button _inspectorBtn;
        [SerializeField] private Button _hierarchyBtn;
        [SerializeField] private Button _prefabsBtn;
        [SerializeField] private Button _transformPositionBtn;
        [SerializeField] private Button _transformRotationBtn;
        [SerializeField] private Button _transformScaleBtn;
        [SerializeField] private List<GameObject> _sceneLoadedMenu;

        private void Start()
        {
            _play.onClick.AddListener(() =>
            {
                RuntimeEditorController.PlayScene();
            });
            
            _undo.onClick.AddListener(() =>
            {
                CommandProcessor.Undo();
            });
        
            _redo.onClick.AddListener(() =>
            {
                CommandProcessor.Redo();
            });

            _new.onClick.AddListener(() =>
            {
                RuntimeEditorController.CreateScene();
            });

            _save.onClick.AddListener(() =>
            {
                RuntimeEditorController.SaveScene();
            });
            
            _load.OnSubmit(data =>
            {
                RuntimeEditorController.LoadScene(TGuid<DataScene.TDataSceneId>.Create(data.Value));
            });
        
            _scene.onClick.AddListener(() =>
            {
                RuntimeEditorController.SelectionBehaviour.AddToSelection(DataScene);
            });

            _inspectorBtn.onClick.AddListener(() =>
            {
                DispatchUiEvent(new EditorUiBehaviour.ToggleWindowEvent{Window = typeof(InspectorWindowBehaviour)});
            });

            _hierarchyBtn.onClick.AddListener(() =>
            {
                DispatchUiEvent(new EditorUiBehaviour.ToggleWindowEvent{Window = typeof(HierarchyWindowBehaviour)});
            });
            
            _prefabsBtn.onClick.AddListener(() =>
            {
                DispatchUiEvent(new EditorUiBehaviour.ToggleWindowEvent{Window = typeof(ProjectWindowBehaviour)});
            });
            
            _transformPositionBtn.onClick.AddListener(() =>
            {
                DispatchUiEvent(new SelectionBehaviour.SetTransformHandleType{HandleType = HandleType.POSITION});
            });
            
            _transformRotationBtn.onClick.AddListener(() =>
            {
                DispatchUiEvent(new SelectionBehaviour.SetTransformHandleType{HandleType = HandleType.ROTATION});
            });

            _transformScaleBtn.onClick.AddListener(() =>
            {
                DispatchUiEvent(new SelectionBehaviour.SetTransformHandleType{HandleType = HandleType.SCALE});
            });

            RefreshScenesList();
            RegisterUiEvent<RuntimeEditorSceneControllerBehaviour.OnSceneLoadedEvent>(RefreshScenesList);
            RegisterUiEvent<RuntimeEditorBehaviour.OnSceneSetToTabEvent>(Redraw);
            
            Redraw();
        }

        private void RefreshScenesList()
        {
            _load.Clear();
            var scenes = RuntimeEditorBehaviour.PersistentDataScenes.LoadData();
            if (scenes != null)
                foreach (var it in scenes)
                    _load.AddData(it.Guid.ToStringRawValue(), it.Name);
        }
        
        private void Redraw()
        {
            var isSceneLoaded = RuntimeEditorController.IsSceneLoaded;
            foreach (var it in _sceneLoadedMenu)
                it.SetActive(isSceneLoaded);
            
            GetComponentInParent<CanvasRebuilderBehaviour>()?.Rebuild();
        }
    }
}


