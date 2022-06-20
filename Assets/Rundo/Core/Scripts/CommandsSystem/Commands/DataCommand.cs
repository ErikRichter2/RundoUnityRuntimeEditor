using Rundo.Core.Data;
using Rundo.Core.Events;
using Rundo.RuntimeEditor.Data;

namespace Rundo.Core.Commands
{
    public abstract class DataCommand<T> : DataCommand, IDataCommand<T>
    {
        public T Data => (T)_data;
        
        protected DataCommand(object data) : base(data) {}
    }

    public abstract class DataCommand : Command
    {
        protected readonly object _data;

        protected DataCommand(object data)
        {
            _data = data;

            if (_data is ICommandProcessorGetter commandProcessorGetter)
                CommandProcessor = commandProcessorGetter.GetCommandProcessor();
            else if (_data is IParentable parentable)
                CommandProcessor = parentable.GetParentInHierarchy<ICommandProcessorGetter>()?.GetCommandProcessor();
        }

        protected override void DispatchInternal(bool wasProcessed)
        {
            base.DispatchInternal(wasProcessed);
            EventDispatcher?.Dispatch(_data, wasProcessed);

            if (EventDispatcher != null)
            {
                if (_data is ICustomDataDispatcher customDataDispatcher)
                    customDataDispatcher.DispatchEvent(EventDispatcher, wasProcessed);
                else if (_data is IParentable parentable)
                    parentable.GetParentInHierarchy<ICustomDataDispatcher>()?.DispatchEvent(EventDispatcher, wasProcessed);
            }
            
        }
    }
}

