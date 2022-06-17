using System;
using System.Collections;
using System.Collections.Generic;

namespace Rundo.Core.Commands
{
    public class RemoveFromListCommandEvent
    {
        public readonly object Data;
        
        public RemoveFromListCommandEvent(object data)
        {
            Data = data;
        }
    }
    
    public class AddToListCommand : DataCommand<IList>
    {
        private readonly List<int> _indexes = new List<int>();
        private readonly List<object> _values = new List<object>();

        public AddToListCommand(IList list, List<int> indexes, List<object> values) : base(list)
        {
            _indexes.AddRange(indexes);
            _values.AddRange(values);
        }
    
        public AddToListCommand(IList list, int index, object value) : base(list)
        {
            _indexes.Add(index);
            _values.Add(value);
        }

        public override ICommand CreateUndo()
        {
            return new RemoveFromListCommand(Data, _indexes);
        }

        protected override void ProcessInternal()
        {
            for (var i = 0; i < _indexes.Count; ++i)
                Data.Insert(_indexes[i], _values[i]);
        }
    }
    
    public class RemoveFromListCommand : DataCommand<IList>
    {
        private readonly List<int> _indexes = new List<int>();

        public RemoveFromListCommand(IList list, int index) : base(list)
        {
            _indexes.Add(index);
        }

        public RemoveFromListCommand(IList list, List<int> indexes) : base(list)
        {
            _indexes.AddRange(indexes);
            _indexes.Sort((a, b) =>
            {
                if (a > b) return -1;
                if (a < b) return 1;
                return 0;
            });
        }

        public override ICommand CreateUndo()
        {
            var indexes = new List<int>();
            indexes.AddRange(_indexes);
            indexes.Sort((a, b) =>
            {
                if (a < b) return -1;
                if (a > b) return 1;
                return 0;
            });

            var values = new List<object>();
            foreach (var index in indexes)
                values.Add(Data[index]);
            
            return new AddToListCommand(Data, indexes, values);
        }

        protected override void ProcessInternal()
        {
            foreach (var index in _indexes)
                Data.RemoveAt(index);
        }
    }
    
    public class SetValueToListCommand : DataCommand<IList>
    {
        private readonly int _index;
        private readonly object _value;
    
        public SetValueToListCommand(IList obj, int index, object value) : base(obj)
        {
            if (obj == null)
                throw new Exception($"Modifying object cant be null");

            _index = index;
            _value = value;
        }

        public override ICommand CreateUndo()
        {
            return new SetValueToListCommand(Data, _index, Data[_index]);
        }

        protected override void ProcessInternal()
        {
            Data[_index] = _value;
        }
    }

}

