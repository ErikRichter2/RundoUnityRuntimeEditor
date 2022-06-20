namespace Rundo.Core.Events
{
    public interface ICustomDataDispatcher
    {
        void DispatchEvent(IEventSystem eventDispatcher, bool wasProcessed);
    }
}

