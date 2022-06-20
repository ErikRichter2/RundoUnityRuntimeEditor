using Rundo.Core.Events;

namespace Rundo.Core.Commands
{
    /**
     * Commands are processed by themselves, they don't require command processor - but command processor (if set)
     * provides option to allow/block commands and provides undo/redo system.
     */
    public interface ICommandProcessor
    {
        IEventSystem EventDispatcher { get; }
        bool CanProcess();
        void AddUndoRedoData(ICommand redoData, ICommand undoData);
        void Undo();
        void Redo();
        void Process(ICommand command);

    }
    
    
}

