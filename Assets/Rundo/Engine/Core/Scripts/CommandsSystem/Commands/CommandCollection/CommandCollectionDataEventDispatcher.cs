using System.Collections.Generic;
using Rundo.Core.EventSystem;

namespace Rundo.Core.Commands
{
    public class CommandCollectionDataEventDispatcher : EventDispatcher
    {
        private readonly List<(object, bool)> _dataToDispatch = new List<(object, bool)>();
        
        public void DispatchThroughDispatcher(IEventDispatcher dispatcher)
        {
            foreach (var data in _dataToDispatch)
                dispatcher.Dispatch(data.Item1, data.Item2);
                
            _dataToDispatch.Clear();
        }

        public override void Dispatch(object data, bool wasProcessed)
        {
            _dataToDispatch.Add((data, wasProcessed));
        }

    }
}

