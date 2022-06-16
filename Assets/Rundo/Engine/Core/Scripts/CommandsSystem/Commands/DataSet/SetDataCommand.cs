using Rundo.Core.Data;

namespace Rundo.Core.Commands
{
    public class SetDataCommand<T> : DataCommand
    {
        private readonly T _child;
        private readonly DataReferenceValueWrapper<T> _dataReferenceValueWrapper;

        public SetDataCommand(object parent, T child, ref DataReference<T> dataReference) : base(parent)
        {
            _child = child;
            _dataReferenceValueWrapper = dataReference.ValueWrapper;
        }
        
        private SetDataCommand(object parent, T child, DataReferenceValueWrapper<T> dataReferenceValueWrapper) : base(parent)
        {
            _child = child;
            _dataReferenceValueWrapper = dataReferenceValueWrapper;
        }

        public override ICommand CreateUndo()
        {
            var undoData = _dataReferenceValueWrapper.Data;
            return new SetDataCommand<T>(_data, undoData, _dataReferenceValueWrapper);
        }
        
        protected override void ProcessInternal()
        {
            if (_dataReferenceValueWrapper.Data is IParentable parentablePrev)
                parentablePrev.SetParent(null);
            _dataReferenceValueWrapper.Set(_child);

            var parentableChild = _child as IParentable;
            var parentableParent = _data as IParentable;
            
            if (parentableChild != null && parentableParent != null)
                parentableChild.SetParent(parentableParent);
        }
    }
}

