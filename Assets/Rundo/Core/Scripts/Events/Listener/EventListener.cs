using System;

namespace Rundo.Core.Events
{
    public class EventListener : IEventListener
    {
        protected readonly IEventSystem _commandDispatcher;
        protected readonly Action _parameterlessListener;
        protected readonly Type _type;
        
        public int Priority { get; private set; }

        public void SetPriority(int priority)
        {
            Priority = priority;
        }

        public EventListener(IEventSystem commandDispatcher, Type type, Action listener)
        {
            _type = type;
            _parameterlessListener = listener;
            _commandDispatcher = commandDispatcher;
        }
        
        public void Dispatch(object data)
        {
            DispatchInternal(data);
        }

        protected virtual void DispatchInternal(object data)
        {
            if (data is Type type)
            {
                if (_type.IsAssignableFrom(type))
                {
                    _parameterlessListener?.Invoke();
                }
            }
            else if (_type.IsAssignableFrom(data.GetType()))
            {
                _parameterlessListener?.Invoke();
            }
        }

        public void Remove()
        {
            _commandDispatcher.Unregister(this);
        }

        public bool IsListener(object callback)
        {
            return IsListenerInternal(callback);
        }

        protected virtual bool IsListenerInternal(object callback)
        {
            if (callback is Action a)
                return _parameterlessListener == a;
            return false;
        }
    }
        
    /**
     * Maps a single callback to a data. Data is mostly a SerializedData instance (which was changed in the command),
     * or command itself.
     */
    public class EventListener<T> : EventListener
    {
        private readonly Action<T> _listener;
        private readonly T _data;
        private readonly bool _isDataSet;

        public EventListener(IEventSystem commandDispatcher, Action<T> listener, T data) : base(commandDispatcher, typeof(T), null)
        {
            _isDataSet = true;
            _data = data;
            _listener = listener;
        }
        
        public EventListener(IEventSystem commandDispatcher, Action<T> listener) : base(commandDispatcher, typeof(T), null)
        {
            _listener = listener;
        }

        protected override void DispatchInternal(object data) 
        {
            if (_isDataSet)
            {
                if (ReferenceEquals(data, _data))
                {
                    _listener?.Invoke(_data);
                    _parameterlessListener?.Invoke();
                }
            }
            else if (data is T t)
            {
                _listener?.Invoke(t);
                _parameterlessListener?.Invoke();
            }
        }

        protected override bool IsListenerInternal(object callback)
        {
            if (callback is Action<T> t)
                return _listener == t;
            return base.IsListenerInternal(callback);
        }
    }
}

