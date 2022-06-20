using System;
using System.Collections;
using System.Collections.Generic;
using Rundo.Core.Data;

namespace Rundo.Core.Commands
{
    public abstract class RemoveDataFromCollectionCommand : DataCommand
    {
        public static RemoveDataFromCollectionCommand Instantiate<TData, TChild>(
            TData collectionOwner, 
            Func<TData, IList<TChild>> collectionGetter, 
            object child)
        {
            var types = new [] {typeof(TData), child.GetType()};
            var args = new [] {collectionOwner, child, collectionGetter.Invoke(collectionOwner) };
            var genericType = typeof(RemoveDataFromCollectionCommand<,>).MakeGenericType(types);
            return (RemoveDataFromCollectionCommand)RundoEngine.DataFactory.Instantiate(genericType, null, args);
        }

        protected RemoveDataFromCollectionCommand(object data) : base(data) {}
    }
    
    public class RemoveDataFromCollectionCommand<TParent, TChild> : RemoveDataFromCollectionCommand, 
        IDataCommand<TParent>,
        ICollectionModifierChild<TChild>, 
        ICollectionModifierParent<TParent> 
    {
        private readonly TChild _childData;
        private readonly IList _collection;
        
        public object CollectionOwner => _data;
        public IList Collection => _collection;
        public TChild Child => _childData;
        public TParent Parent => (TParent)_data;
        
        public RemoveDataFromCollectionCommand(
            TParent parentData, 
            TChild childData, 
            IList collection) : base(parentData)
        {
            _childData = childData;
            _collection = collection;
        }

        public override ICommand CreateUndo()
        {
            return new AddDataToCollectionCommand<TParent, TChild>(
                (TParent)_data, _childData, _collection, _collection.IndexOf(_childData));
        }
        
        protected override void ProcessInternal()
        {
            _collection.Remove(_childData);
            
            if (_childData is IParentable parentable)
                parentable.SetParent(null);
        }
    }
}

