using System;
using System.Collections.Generic;

namespace Rundo.Core.Events
{
    /**
     * Standard implementation of ICommandDispatcher.
     */
    public class EventSystem : IEventSystem
    {
        /**
         * These callbacks are dispatched even when the command was not processed - to force UI/world redraw.
         * For example if the level is locked we can still set values to UI elements, or use world transform gizmo -
         * these actions will generate a command which is not processed (level is read-only) so the datamodel is not
         * changed, but we still want to force UI/world to refresh its state so the UI/world values return back to the
         * model state.
         */
        private readonly List<IEventListener> _listeners = new List<IEventListener>();
        
        /**
         * These callbacks are dispatched only when the command was successfully processed.
         */
        private readonly List<IEventListener> _listenersOnlyWhenProcessed = new List<IEventListener>();

        /**
         * All events are dispatched through these external dispatcher as well - for example, each workspace (level)
         * has its own command processor and command dispatcher (so levels are not affecting each other), but we still
         * want to listen to all commands from all levels - we have a editor-context dispatcher which is then injected
         * to each workspace (level) dispatcher.
         */
        private readonly List<IEventSystem> _externalDispatchers = new List<IEventSystem>();

        public IEventListener Register(Type type, Action listener)
        {
            var commandListener = new EventListener(this, type, listener);
            _listeners.Add(commandListener);
            return commandListener;
        }

        public IEventListener Register<T>(Action callback)
        {
            var commandListener = new EventListener(this, typeof(T), callback);
            _listeners.Add(commandListener);
            return commandListener;
        }
        
        public IEventListener Register<T>(Action<T> callback, bool onlyWhenProcessed)
        {
            Unregister(callback);
            var commandListener = new EventListener<T>(this, callback);
            
            if (onlyWhenProcessed)
                _listenersOnlyWhenProcessed.Add(commandListener);
            else
                _listeners.Add(commandListener);
            
            return commandListener;
        }

        public IEventListener Register<T>(Action<T> callback)
        {
            Unregister(callback);
            var commandListener = new EventListener<T>(this, callback);
            _listeners.Add(commandListener);
            return commandListener;
        }

        public IEventListener Register<T>(Action<T> listener, T data)
        {
            var commandListener = new EventListener<T>(this, listener, data);
            _listeners.Add(commandListener);
            return commandListener;
        }

        public void Unregister(Action listener)
        {
            foreach (var it in _listeners)
                if (it.IsListener(listener))
                {
                    _listeners.Remove(it);
                    return;
                }
            
            foreach (var it in _listenersOnlyWhenProcessed)
                if (it.IsListener(listener))
                {
                    _listenersOnlyWhenProcessed.Remove(it);
                    return;
                }
        }

        public void Unregister<T>(Action<T> callback)
        {
            foreach (var it in _listeners)
                if (it.IsListener(callback))
                {
                    _listeners.Remove(it);
                    return;
                }
            
            foreach (var it in _listenersOnlyWhenProcessed)
                if (it.IsListener(callback))
                {
                    _listenersOnlyWhenProcessed.Remove(it);
                    return;
                }
        }

        public void Unregister(IEventListener listener)
        {
            _listeners.Remove(listener);
            _listenersOnlyWhenProcessed.Remove(listener);
        }

        public void Dispatch(object data)
        {
            Dispatch(data, true);
        }

        private void DispatchByPriority(object data, List<IEventListener> listeners)
        {
            listeners.Sort((a, b) =>
            {
                if (a.Priority > b.Priority)
                    return -1;
                if (a.Priority < b.Priority)
                    return 1;
                return 0;
            });
            
            foreach (var it in listeners.ToArray())
                it.Dispatch(data);
        }
        
        public virtual void Dispatch(object data, bool wasProcessed)
        {
            DispatchByPriority(data, _listeners);
            
            if (wasProcessed)
                DispatchByPriority(data, _listenersOnlyWhenProcessed);
                    
            foreach (var externalDispatcher in _externalDispatchers)
                externalDispatcher.Dispatch(data, wasProcessed);
        }

        public void AddExternalEventSystem(IEventSystem dispatcher)
        {
            if (_externalDispatchers.Contains(dispatcher))
                return;
            _externalDispatchers.Add(dispatcher);
        }
        
        public void UnregisterAll()
        {
            _listeners.Clear();
            _listenersOnlyWhenProcessed.Clear();
            _externalDispatchers.Clear();
        }
    }
}

