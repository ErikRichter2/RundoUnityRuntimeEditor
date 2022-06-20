using System;
using Rundo.Core.Events;

namespace Rundo.Core.Commands
{
    public class ReadOnlyCommandProcessor : ICommandProcessor
    {
        private static ReadOnlyCommandProcessor _singleton;

        public static ReadOnlyCommandProcessor Instance
        {
            get
            {
                _singleton ??= new ReadOnlyCommandProcessor();
                return _singleton;
            }
        }
        
        public void AddUndoRedoData(ICommand redoData, ICommand undoData)
        {
            throw new NotImplementedException();
        }

        public void OnCommandProcessed<T>(T data)
        {
            throw new NotImplementedException();
        }

        public IEventSystem EventDispatcher { get; }

        public bool CanProcess()
        {
            return false;
        }

        public void Undo()
        {
            throw new NotImplementedException();
        }

        public void Redo()
        {
            throw new NotImplementedException();
        }

        public void Process(ICommand command)
        {
            throw new NotImplementedException();
        }
    }
}

