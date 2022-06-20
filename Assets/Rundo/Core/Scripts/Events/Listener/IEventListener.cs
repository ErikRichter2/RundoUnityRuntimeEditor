namespace Rundo.Core.Events
{
    public interface IEventListener
    {
        void Dispatch(object data);
        void Remove();
        bool IsListener(object callback);
        void SetPriority(int priority);
        int Priority { get; }
    }
}

