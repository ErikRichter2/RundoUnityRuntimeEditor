namespace Rundo.Core.Commands
{
    public interface ICommandProcessorProvider : ICommandProcessorGetter
    {
        public ICommandProcessor CommandProcessor { get; }
    }

    public interface ICommandProcessorGetter
    {
        public ICommandProcessor GetCommandProcessor();
    }
}

