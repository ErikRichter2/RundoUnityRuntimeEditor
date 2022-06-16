using System;

namespace Rundo.Core.Commands
{
    public class ModifyDataCommand : DataCommand
    {
        public object PreviousData { get; private set; }
        private readonly string _modifiedData;

        public ModifyDataCommand(object data, string modifiedData) : base(data)
        {
            _modifiedData = modifiedData;
        }

        public override ICommand CreateUndo()
        {
            return new ModifyDataCommand(_data, RundoEngine.DataSerializer.SerializeObject(_data));
        }

        protected override void ProcessInternal()
        {
            PreviousData = RundoEngine.DataSerializer.Clone(_data);
            RundoEngine.DataSerializer.Populate(_modifiedData, _data);
        }
    }
    
    public class ModifyDataCommand<T> : DataCommand
    {
        public T Data => (T)_data;

        public T PreviousData { get; private set; }

        private readonly string _modifiedData;

        public ModifyDataCommand(T data, Action<T> modifier) : base(data)
        {
            var copyForModify = (T)RundoEngine.DataSerializer.Clone(data);
            modifier?.Invoke(copyForModify);
            _modifiedData = RundoEngine.DataSerializer.SerializeObject(copyForModify);
        }

        public ModifyDataCommand(T serializedData, string modifiedData) : base(serializedData)
        {
            _modifiedData = modifiedData;
        }

        public override ICommand CreateUndo()
        {
            return new ModifyDataCommand<T>(Data, RundoEngine.DataSerializer.SerializeObject(Data));
        }

        protected override void ProcessInternal()
        {
            PreviousData = (T)RundoEngine.DataSerializer.Clone(Data);
            RundoEngine.DataSerializer.Populate(_modifiedData, Data);
        }
    }
}

