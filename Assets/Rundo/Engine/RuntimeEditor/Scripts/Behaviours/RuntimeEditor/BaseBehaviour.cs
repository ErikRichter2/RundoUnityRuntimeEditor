using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Rundo.Core.Commands;
using Rundo.Core.EventSystem;
using Rundo.RuntimeEditor.Data;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// The most base behaviour class that provides a shortcut for most used functions within the runtime editor context
    /// </summary>
    public class BaseBehaviour : MonoBehaviour
    {
        private IBaseDataProviderBehaviour _baseDataProvider;

        [JsonIgnore]
        public IBaseDataProviderBehaviour BaseDataProvider
        {
            get
            {
                _baseDataProvider ??= GetComponentInParent<IBaseDataProviderBehaviour>();
                return _baseDataProvider;
            }
        }

        [JsonIgnore]
        public DataScene DataScene => BaseDataProvider?.GetDataScene();
        [JsonIgnore]
        public ICommandProcessor CommandProcessor => BaseDataProvider?.GetCommandProcessor();
        [JsonIgnore]
        protected IEventDispatcher CommandDispatcher => CommandProcessor?.EventDispatcher;
        [JsonIgnore]
        protected EventDispatcher UiEventsDispatcher => BaseDataProvider?.GetUiEventDispatcher();
    
        private readonly List<IEventListener> _eventListeners = new List<IEventListener>();
        
        public IEventListener RegisterUiEvent<T>(Action<T> callback) where T: IUiEvent
        {
            var eventListener = UiEventsDispatcher.Register(callback);
            _eventListeners.Add(eventListener);
            return eventListener;
        }

        public IEventListener RegisterUiEvent<T>(Action callback) where T: IUiEvent
        {
            var eventListener = UiEventsDispatcher.Register<T>(callback);
            _eventListeners.Add(eventListener);
            return eventListener;
        }

        public void DispatchUiEvent<T>(T data) where T: IUiEvent
        {
            UiEventsDispatcher.Dispatch(data);
        }
        
        public void RegisterCommandListener<T>(Action<T> callback)
        {
            var listener = CommandDispatcher.Register<T>(callback);
            _eventListeners.Add(listener);
        }

        public void UnregisterCommandListener<T>(Action<T> callback)
        {
            foreach (var it in _eventListeners)
                if (it.IsListener(callback))
                {
                    it.Remove();
                    _eventListeners.Remove(it);
                    return;
                }
        }

        public void RegisterCommandListener<T>(Action callback)
        {
            var listener = CommandDispatcher.Register<T>(callback);
            _eventListeners.Add(listener);
        }
    
        private void OnDestroy()
        {
            foreach (var listener in _eventListeners)
                listener.Remove();
            _eventListeners.Clear();
    
            OnDestroyInternal();
        }
    
        protected virtual void OnDestroyInternal() {}
    }
}


