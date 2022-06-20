using System;
using System.Collections.Generic;
using Rundo.Core.Events;

namespace Rundo.Core.Commands
{
    public abstract class Command : ICommand
    {
        private static bool _isCommandProcessing;
        
        public ICommandProcessor CommandProcessor { get; set; }

        private IEventSystem _eventDispatcher;
        public IEventSystem EventDispatcher
        {
            get => _eventDispatcher ?? CommandProcessor?.EventDispatcher;
            set => _eventDispatcher = value;
        }

        public bool IgnoreUndoRedo { get; set; }

        public abstract ICommand CreateUndo();

        private List<object> _dispatcherData;
        private List<Type> _dispatcherTypes;
        
        public ICommand AddDispatcherData<T>(T data)
        {
            _dispatcherData ??= new List<object>();
            _dispatcherData.Add(data);
            return this;
        }

        public void AddDispatcherType(Type type)
        {
            _dispatcherTypes ??= new List<Type>();
            _dispatcherTypes.Add(type);
        }

        public ICommand SetIgnoreUndoRedo()
        {
            IgnoreUndoRedo = true;
            return this;
        }

        public void Process()
        {
            var commandProcessor = CommandProcessor;

            if (CanProcess() == false)
            {
                Dispatch(false);
                return;
            }

            bool canUndo = CanUndo();

            if (_isCommandProcessing)
                canUndo = false;
            
            ICommand undoCommand = null;

            if (canUndo)
            {
                undoCommand = CreateUndo();
                if (_dispatcherData != null)
                    foreach (var it in _dispatcherData)
                        undoCommand.AddDispatcherData(it);
                if (_dispatcherTypes != null)
                    foreach (var it in _dispatcherTypes)
                        undoCommand.AddDispatcherType(it);
            }

            _isCommandProcessing = true;
            ProcessInternal();
            _isCommandProcessing = false;
            
            if (canUndo)
                commandProcessor.AddUndoRedoData(this, undoCommand);
            
            Dispatch(true);
        }

        protected abstract void ProcessInternal();

        private void Dispatch(bool wasProcessed)
        {
            if (EventDispatcher != null)
            {
                EventDispatcher.Dispatch(this, wasProcessed);
                if (_dispatcherData != null)
                    foreach (var it in _dispatcherData)
                        EventDispatcher.Dispatch(it, wasProcessed);
                if (_dispatcherTypes != null)
                    foreach (var it in _dispatcherTypes)
                        EventDispatcher.Dispatch(it, wasProcessed);
                DispatchInternal(wasProcessed);
            }
        }

        protected virtual void DispatchInternal(bool wasProcessed) {}

        protected virtual bool CanProcess()
        {
            return CommandProcessor == null || CommandProcessor.CanProcess();
        }

        private bool CanUndo()
        {
            return CommandProcessor != null && IgnoreUndoRedo == false;
        }
    }
}

