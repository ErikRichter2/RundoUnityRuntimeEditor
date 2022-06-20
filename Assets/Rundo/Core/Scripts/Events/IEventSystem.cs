using System;

namespace Rundo.Core.Events
{
    /**
     * Handles command events - each command generates events which are dispatched through implementation of this
     * interface. It is separated from the command processor to provide custom dispatcher for example for
     * command-collection command which runs a collection of commands but the events should be postponed until all
     * commands are processed and then dispatched at once. So command-collection command creates its own command
     * dispatcher which is injected into its collection of commands which pickups events without dispatching them
     * until all commands are processed.
     *
     * Listeners are type-checked so for example if we have a base class LaserSessionData and extended classes
     * LaserFilterSessionData and LaserMirrorSessionData then using RegisterCommandListener<LaserSessionData> will
     * listen to commands for both implementations (so ModifySerializedData<LaserFilterSessionData> as well as
     * ModifySerializedData<LaserMirrorSessionData>).
     *
     * We can also listen to a command itself - RegisterCommandListener<ModifySerializedData<LaserSessionData>>, which
     * will dispatch the command data (if we want to compare current data state with previous data state for example).
     */
    public interface IEventSystem
    {
        void Dispatch(object data, bool wasProcessed);
        IEventListener Register(Type type, Action listener);
        IEventListener Register<T>(Action listener);
        IEventListener Register<T>(Action<T> listener);
        IEventListener Register<T>(Action<T> listener, T data);
        void Unregister(IEventListener listener);
        void AddExternalEventSystem(IEventSystem dispatcher);
        IEventListener Register<T>(Action<T> callback, bool onlyWhenProcessed);
        void Unregister(Action listener);
        void Unregister<T>(Action<T> callback);
        void UnregisterAll();
    }
}

