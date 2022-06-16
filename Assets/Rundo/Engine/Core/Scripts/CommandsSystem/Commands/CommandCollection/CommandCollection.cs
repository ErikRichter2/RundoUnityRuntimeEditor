using System;
using System.Collections.Generic;

namespace Rundo.Core.Commands
{
    public class CommandCollection : Command
    {
        private readonly List<ICommand> _commands = new List<ICommand>();

        private readonly CommandCollectionDataEventDispatcher _commandCollectionDataCommandDispatcher =
            new CommandCollectionDataEventDispatcher();
        
        private readonly List<Action> _beforeDispatchCallbacks = new List<Action>();

        public CommandCollection(ICommandProcessor commandProcessor)
        {
            CommandProcessor = commandProcessor;
        }

        public void AddBeforeDispatchCallback(Action callback)
        {
            _beforeDispatchCallbacks.Add(callback);
        }

        public override ICommand CreateUndo()
        {
            var commandCollection = new CommandCollection(CommandProcessor);
            
            for (var i = _commands.Count - 1; i >= 0; --i)
                commandCollection.AddCommand(_commands[i].CreateUndo());
            
            foreach (var callback in _beforeDispatchCallbacks)
                commandCollection.AddBeforeDispatchCallback(callback);
            
            return commandCollection;
        }
        
        protected override void ProcessInternal()
        {
            foreach (var command in _commands)
            {
                command.IgnoreUndoRedo = true;
                command.CommandProcessor = CommandProcessor;
                command.EventDispatcher = _commandCollectionDataCommandDispatcher;
                command.Process();
            }
        }
        
        protected override void DispatchInternal(bool wasProcessed)
        {
            base.DispatchInternal(wasProcessed);

            foreach (var callback in _beforeDispatchCallbacks)
                callback?.Invoke();

            _commandCollectionDataCommandDispatcher.DispatchThroughDispatcher(CommandProcessor.EventDispatcher);
        }

        public void AddCommand(ICommand command)
        {
            _commands.Add(command);
        }
    }

}

