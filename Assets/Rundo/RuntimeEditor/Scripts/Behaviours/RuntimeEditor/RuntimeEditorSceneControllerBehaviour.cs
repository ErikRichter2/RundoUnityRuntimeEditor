using System.Collections.Generic;
using System.Threading.Tasks;
using Rundo.Core.Commands;
using Rundo.Core.Data;
using Rundo.Core.Events;
using Rundo.RuntimeEditor.Behaviours.UI;
using Rundo.RuntimeEditor.Data;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// Represents a single-scene editor tab.
    /// </summary>
    public class RuntimeEditorSceneControllerBehaviour : EditorBaseBehaviour, IBaseDataProviderBehaviour
    {
        [SerializeField] private FreeCameraController _freeCamera;
        [SerializeField] private TopDownCameraController _topDownCamera;
        [SerializeField] private GameObject _editorMode;
        [SerializeField] private RuntimeEditorPlayModeControllerBehaviour _playMode;
        
        public struct OnSceneLoadedEvent : IUiEvent {}
        
        public struct OnSceneDeletedEvent : IUiEvent
        {
            public TGuid<RuntimeEditorBehaviour.TRuntimeEditorTab> TabGuid;
            public TGuid<DataScene.TDataSceneId> SceneId;
        }
        
        public struct OnPlayModeChanged : IUiEvent {}

        /// <summary>
        /// Keeps reference to the DataScene instance
        /// </summary>
        private DataScene _dataScene;

        private Camera _activeCamera;

        private readonly Dictionary<Camera, Vector3> _defaultCameraPosition = new Dictionary<Camera, Vector3>();
        private readonly Dictionary<Camera, Vector3> _defaultCameraRotation = new Dictionary<Camera, Vector3>();

        private readonly CommandProcessor _commandProcessor = new CommandProcessor();
        public readonly EventSystem UiEvents = new EventSystem();
        
        public bool IsEditorMode { get; private set; }

        private DataSceneMetaData _dataSceneMetaData;
        public DataSceneMetaData DataSceneMetaData
        {
            get
            {
                if (_dataScene != null)
                    return _dataScene.DataSceneMetaData;
                return _dataSceneMetaData;
            }
            private set => _dataSceneMetaData = value;
        }
        
        public TGuid<RuntimeEditorBehaviour.TRuntimeEditorTab> TabGuid { get; } =
            TGuid<RuntimeEditorBehaviour.TRuntimeEditorTab>.Create();
        
        public Camera ActiveCamera
        {
            get
            {
                if (_activeCamera == null)
                    return Camera.main;
                return _activeCamera;
            }
            private set
            {
                if (_defaultCameraPosition.ContainsKey(value) == false)
                {
                    _defaultCameraPosition[value] = value.transform.position;
                    _defaultCameraRotation[value] = value.transform.localEulerAngles;
                }
                
                _activeCamera = value;
            }
        }
        
        private EditorModeBaseBehaviour _currentMode;
        private bool _isLazyLoad;

        public DataSceneBehaviour DataSceneBehaviour { get; private set; }
        public SelectionBehaviour SelectionBehaviour { get; private set; }

        public bool IsSceneLoaded => _dataScene != null;

        private void Start()
        {
            if (_isLazyLoad)
                LoadScene(_dataSceneMetaData.Guid);
            CreateUi();
            CreateWorld();
            StopPlayScene();
        }

        private void CreateUi()
        {
            var uiPrefab = Resources.Load<EditorUiBehaviour>("Rundo/RuntimeEditor/RuntimeEditorUi");
            Instantiate(uiPrefab, _editorMode.transform);
        }

        private void CreateWorld()
        {
            SelectionBehaviour ??= _editorMode.AddComponent<SelectionBehaviour>();
            SelectionBehaviour.ClearSelection();

            if (DataSceneBehaviour != null)
                Destroy(DataSceneBehaviour.gameObject);
            
            var dataSceneGo = new GameObject("DataScene");
            dataSceneGo.transform.SetParent(_editorMode.transform, true);
            DataSceneBehaviour = dataSceneGo.AddComponent<DataSceneBehaviour>();
            
            SetTopDownCamera();

            SetMode<SelectObjectsEditorModeBehaviour>();
        }

        public void LazyLoadScene(DataSceneMetaData dataSceneMetaData)
        {
            _isLazyLoad = true;
            _dataSceneMetaData = dataSceneMetaData;
            RuntimeEditor.DispatchUiEventToAllSceneControllers(new RuntimeEditorBehaviour.OnSceneSetToTabEvent());
        }

        public void LoadScene(TGuid<DataScene.TDataSceneId> sceneId)
        {
            LoadSceneInternal(sceneId);
        }

        public void CreateScene()
        {
            SetScene(RundoEngine.DataFactory.Instantiate<DataScene>());
        }
        
        public void DeleteScene()
        {
            if (_dataScene != null)
            {
                RuntimeEditorBehaviour.PersistentDataScenes.DeleteData(_dataScene.DataSceneMetaData);
                SetScene(default);
            }
        }

        private void SetTopDownCamera()
        {
            _freeCamera.gameObject.SetActive(false);
            _topDownCamera.gameObject.SetActive(true);
            ActiveCamera = _topDownCamera.GetComponent<Camera>();
        }

        public void SetFreeCamera()
        {
            _freeCamera.gameObject.SetActive(true);
            _topDownCamera.gameObject.SetActive(false);
            ActiveCamera = _freeCamera.GetComponent<Camera>();
        }
    
        public T SetMode<T>() where T : EditorModeBaseBehaviour
        {
            if (_currentMode != null)
            {
                _currentMode.Deactivate();
                Destroy(_currentMode);
            }
    
            _currentMode = gameObject.AddComponent<T>();
            _currentMode.Activate();
            return (T)_currentMode;
        }
        
        public void SetDefaultCameraTransform(Camera camera)
        {
            if (_defaultCameraPosition.ContainsKey(camera))
            {
                camera.transform.position = _defaultCameraPosition[camera];
                camera.transform.localEulerAngles = _defaultCameraRotation[camera];
            }
        }

        public void SaveScene()
        {
            if (_dataScene == null)
                return;
            
            RuntimeEditorBehaviour.PersistentDataScenes.SaveData(_dataScene.DataSceneMetaData, _dataScene);

            var editorPrefs = RuntimeEditorBehaviour.PersistentEditorPrefs.LoadData();
            if (editorPrefs.OpenedScenes.Contains(_dataScene.DataSceneMetaData.Guid.ToStringRawValue()) == false)
            {
                editorPrefs.OpenedScenes.Add(_dataScene.DataSceneMetaData.Guid.ToStringRawValue());
                RuntimeEditorBehaviour.PersistentEditorPrefs.SaveData(editorPrefs);
            }
        }

        private void LoadSceneInternal(TGuid<DataScene.TDataSceneId> sceneId)
        {
            SetScene(RuntimeEditorBehaviour.PersistentDataScenes.LoadData(sceneId.ToStringRawValue()));
        }
        
        private void SetScene(DataScene dataScene)
        {
            _isLazyLoad = false;

            _dataScene = dataScene;
            _dataSceneMetaData = _dataScene?.DataSceneMetaData ?? default;

            _commandProcessor.ClearUndoRedo();
            
            _dataScene?.SetCommandProcessor(CommandProcessor);
            
            DispatchUiEvent(new OnSceneLoadedEvent());
            RuntimeEditor.DispatchUiEventToAllSceneControllers(new RuntimeEditorBehaviour.OnSceneSetToTabEvent());
            
            CreateWorld();
        }

        public DataScene GetDataScene()
        {
            return _dataScene;
        }

        public async Task<PrefabIdBehaviour> LoadPrefab(TGuid<TPrefabId> prefabId)
        {
            await Task.Yield();
            return RuntimeEditor.GetPrefab(prefabId);
        }

        public void PostprocessGameObject(GameObject gameObject)
        {
            if (gameObject.GetComponent<EditorRaycastHitColliderHandlerBehaviour>() == null)
                gameObject.AddComponent<EditorRaycastHitColliderHandlerBehaviour>();
        }

        public CommandProcessor GetCommandProcessor()
        {
            return _commandProcessor;
        }

        public EventSystem GetUiEventDispatcher()
        {
            return UiEvents;
        }

        public void PlayScene()
        {
            if (_dataScene == null)
                return;
            
            IsEditorMode = false;
            _playMode.SetData(RundoEngine.DataSerializer.Copy(_dataScene), StopPlayScene);
            _playMode.gameObject.SetActive(true);
            _editorMode.SetActive(false);
            DispatchUiEvent(new OnPlayModeChanged());
        }

        private void StopPlayScene()
        {
            IsEditorMode = true;
            _playMode.gameObject.SetActive(false);
            _editorMode.SetActive(true);
            DispatchUiEvent(new OnPlayModeChanged());
        }
    }
}



