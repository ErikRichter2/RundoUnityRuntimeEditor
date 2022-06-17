using System.Collections.Generic;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data;
using Rundo.Core.Utils;
using Rundo.RuntimeEditor.Tools;
using Rundo.Ui;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// The root class of the runtime editor. Handles views (tabs) of scenes.
    /// </summary>
    public class RuntimeEditorBehaviour : MonoBehaviour
    {
        public static PersistentDataSet<DataSceneMetaData, DataScene> PersistentDataScenes = new PersistentDataSet<DataSceneMetaData, DataScene>("RuntimeEditorScenes");
        public static PersistentData<RuntimeEditorPrefs> PersistentEditorPrefs = new PersistentData<RuntimeEditorPrefs>("RuntimeEditorPrefs", new RuntimeEditorPrefs());
        
        public struct TRuntimeEditorTab {}

        public struct OnTabAddedEvent : IUiEvent {}
        public struct OnTabRemovedEvent : IUiEvent {}
        public struct OnTabSelectedEvent : IUiEvent {}
        public struct OnSceneSetToTabEvent : IUiEvent {}

        [SerializeField] private Transform _tabsContent;
        [SerializeField] private List<string> _prefabsResourcesPaths = new List<string>();
        [SerializeField] private PrefabScreenshoterBehaviour _prefabScreenshoterBehaviour;
        
        public TGuid<TRuntimeEditorTab> SelectedTab { get; private set; }
        
        /// <summary>
        /// Is true when the input is over the world, is false when the input is over the UI
        /// </summary>
        public static bool IsInputOverWorld { get; set; }
        
        public PrefabScreenshoterBehaviour PrefabScreenshoterBehaviour => _prefabScreenshoterBehaviour;
        
        private List<PrefabIdBehaviour> _prefabs;
        
        public readonly List<RuntimeEditorSceneControllerBehaviour> InstantiatedTabs = new List<RuntimeEditorSceneControllerBehaviour>();

        private void Start()
        {
            RundoEngine.DataSerializer.AddDefaultReadConverter(new DataComponentReadJsonConverter());

            var sceneMetaDatas = PersistentDataScenes.LoadData();
            
            // load scenes
            foreach (var it in PersistentEditorPrefs.LoadData().OpenedScenes)
            {
                foreach (var sceneMetaData in sceneMetaDatas)
                {
                    if (sceneMetaData.Guid.ToStringRawValue() == it)
                    {
                        var instance = InstantiateSceneController(_tabsContent);
                        instance.LazyLoadScene(sceneMetaData);
                        InstantiatedTabs.Add(instance);
                    }
                }
            }

            if (InstantiatedTabs.Count == 0)
                AddTab();
            
            SelectTab(InstantiatedTabs[0].TabGuid);
        }

        private RuntimeEditorSceneControllerBehaviour InstantiateSceneController(Transform parent)
        {
            var prefab = Resources.Load<RuntimeEditorSceneControllerBehaviour>("Rundo/RuntimeEditor/RuntimeEditorSceneController");
            return Instantiate(prefab, parent);
        }

        public void SelectTab(TGuid<TRuntimeEditorTab> tabGuid)
        {
            SelectedTab = tabGuid;
            
            foreach (var tab in InstantiatedTabs)
                tab.gameObject.SetActive(tab.TabGuid == SelectedTab);
            
            DispatchUiEventToAllSceneControllers(new OnTabSelectedEvent());
        }

        public void AddTab()
        {
            var tabInstance = InstantiateSceneController(_tabsContent);
            InstantiatedTabs.Add(tabInstance);
            SelectTab(tabInstance.TabGuid);
            DispatchUiEventToAllSceneControllers(new OnTabAddedEvent());
        }

        public void CloseTab(TGuid<TRuntimeEditorTab> tabGuid)
        {
            foreach (var tab in InstantiatedTabs)
            {
                if (tab.TabGuid == tabGuid)
                {
                    InstantiatedTabs.Remove(tab);

                    if (tab.TabGuid == SelectedTab && InstantiatedTabs.Count > 0)
                        SelectTab(InstantiatedTabs[0].TabGuid);
                    else
                        SelectedTab = default;

                    Destroy(tab.gameObject);
                    break;
                }
            }

            DispatchUiEventToAllSceneControllers(new OnTabRemovedEvent());

            if (InstantiatedTabs.Count == 0)
            {
                AddTab();
                SelectTab(InstantiatedTabs[0].TabGuid);
            }
        }

        public PrefabIdBehaviour GetPrefab(TGuid<TPrefabId> prefabId)
        {
            foreach (var prefab in _prefabs)
                if (prefab.Guid == prefabId)
                    return prefab;

            return null;
        }

        public List<PrefabIdBehaviour> GetPrefabs()
        {
            if (_prefabs != null)
                return _prefabs;
            
            _prefabs = new List<PrefabIdBehaviour>();
            foreach (var path in _prefabsResourcesPaths)
                foreach (var prefab in Resources.LoadAll<PrefabIdBehaviour>(path))
                    _prefabs.Add(prefab);

            return _prefabs;
        }

        public void DispatchUiEventToAllSceneControllers(IUiEvent data)
        {
            foreach (var it in InstantiatedTabs)
                it.GetComponentInChildren<RuntimeEditorSceneControllerBehaviour>().UiEvents.Dispatch(data);
        }

        public TGuid<DataScene.TDataSceneId> GetSceneId(TGuid<TRuntimeEditorTab> tabId)
        {
            foreach (var it in InstantiatedTabs)
                if (it.TabGuid == tabId && it.IsSceneLoaded)
                    return it.DataScene.DataSceneMetaData.Guid;
            return default;
        }
    }
}



