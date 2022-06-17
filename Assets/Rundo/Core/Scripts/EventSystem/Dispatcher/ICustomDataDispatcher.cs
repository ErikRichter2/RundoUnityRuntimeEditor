namespace Rundo.Core.EventSystem
{
    public interface ICustomDataDispatcher
    {
        void DispatchEvent(IEventDispatcher eventDispatcher, bool wasProcessed);
    }
}

