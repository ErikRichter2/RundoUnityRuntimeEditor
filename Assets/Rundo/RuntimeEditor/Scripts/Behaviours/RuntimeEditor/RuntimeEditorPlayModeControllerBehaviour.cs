using System;
using System.Threading.Tasks;
using Rundo.Core.Commands;
using Rundo.Core.Data;
using Rundo.Core.Events;
using Rundo.Core.Utils;
using Rundo.RuntimeEditor.Data;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// Represents a editor playmode.
    /// </summary>
    public class RuntimeEditorPlayModeControllerBehaviour : EditorBaseBehaviour, IBaseDataProviderBehaviour
    {
        public struct OnExitPlayModeEvent : IUiEvent {}
        
        private Action _onExitPlayMode;
        private DataScene _dataScene;
        private CommandProcessor _commandProcessor;
        private EventSystem _uiEventDispatcher;

        public virtual void SetData(DataScene dataScene, Action onExitPlayMode)
        {
            _dataScene = dataScene;
            _onExitPlayMode = onExitPlayMode;
            _commandProcessor = new CommandProcessor();
            _uiEventDispatcher = new EventSystem();
            _uiEventDispatcher.Register<OnExitPlayModeEvent>(StopPlayScene);

            Init();
        }

        private void StopPlayScene()
        {
            foreach (Transform it in transform)
                Destroy(it.gameObject);
            
            _onExitPlayMode?.Invoke();
        }

        private void Init()
        {
            // ui
            Instantiate(Resources.Load<EditorPlayModeUiBehaviour>("Rundo/RuntimeEditor/RuntimeEditorPlaymodeUi"), transform);
            
            // world
            var dataSceneGo = new GameObject("DataScene");
            dataSceneGo.transform.SetParent(transform, true);
            dataSceneGo.AddComponent<DataSceneBehaviour>();
            
            // camera
            Instantiate(Resources.Load<GameObject>("Rundo/RuntimeEditor/RuntimeEditorPlaymodeCamera"), transform);
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
        }

        public CommandProcessor GetCommandProcessor()
        {
            return _commandProcessor;
        }

        public EventSystem GetUiEventDispatcher()
        {
            return _uiEventDispatcher;
        }
    }
}



