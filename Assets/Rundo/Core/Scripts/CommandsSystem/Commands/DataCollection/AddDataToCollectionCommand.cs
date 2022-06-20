using System;
using System.Collections;
using System.Collections.Generic;
using Rundo.Core.Data;

namespace Rundo.Core.Commands
{
    public abstract class AddDataToCollectionCommand : DataCommand
    {
        public static AddDataToCollectionCommand Instantiate<TData, TChild>(
            TData collectionOwner, 
            Func<TData, IList<TChild>> collectionGetter, 
            object child, 
            int insertAt = -1)
        {
            var collection = collectionGetter.Invoke(collectionOwner);
            if (insertAt == -1)
                insertAt = collection.Count;
            var types = new Type[] {typeof(TData), child.GetType()};
            var args = new object[] {collectionOwner, child, collection, insertAt };
            var genericType = typeof(AddDataToCollectionCommand<,>).MakeGenericType(types);
            return (AddDataToCollectionCommand)RundoEngine.DataFactory.Instantiate(genericType, null, args);
        }
        
        protected AddDataToCollectionCommand(object data) : base(data) {}
    }
    
    public class AddDataToCollectionCommand<TParent, TChild> : AddDataToCollectionCommand, 
        IDataCommand<TParent>,
        ICollectionModifierChild<TChild>, 
        ICollectionModifierParent<TParent>
    {
        private readonly TChild _childData;
        private readonly IList _collection;
        private readonly int _insertAt;
        
        public object CollectionOwner => _data;
        public IList Collection => _collection;
        public TChild Child => _childData;
        public TParent Parent => (TParent)_data;
        
        public AddDataToCollectionCommand(
            TParent parentData, 
            TChild childData, 
            IList collection, 
            int insertAt = -1) : base(parentData)
        {
            _childData = childData;
            _collection = collection;
            _insertAt = insertAt;
        }

        public override ICommand CreateUndo()
        {
            return new RemoveDataFromCollectionCommand<TParent, TChild>(
                (TParent)_data, _childData, _collection);
        }

        protected override void ProcessInternal()
        {
            if (_insertAt == -1)
                _collection.Add(_childData);
            else
                _collection.Insert(_insertAt, _childData);

            var parentableChild = _childData as IParentable;
            var parentableParent = _data as IParentable;
            
            if (parentableChild != null && parentableParent != null)
                parentableChild.SetParent(parentableParent);
        }
    }
}

