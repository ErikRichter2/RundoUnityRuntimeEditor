using System;
using Rundo.Core.EventSystem;

namespace Rundo.Core.Commands
{
    public interface ICommand
    {
        ICommand CreateUndo();
        void Process();
        bool IgnoreUndoRedo { get; set; }
        ICommand AddDispatcherData<T>(T data);
        void AddDispatcherType(Type type);
        
        ICommandProcessor CommandProcessor { get; set; }
        IEventDispatcher EventDispatcher { get; set; }
    }
}

