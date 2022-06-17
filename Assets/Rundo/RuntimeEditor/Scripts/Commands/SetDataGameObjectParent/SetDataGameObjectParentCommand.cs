using System;
using Rundo.Core.Commands;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data;
using UnityEngine.Assertions;

namespace Rundo.RuntimeEditor.Commands
{
    public class SetDataGameObjectParentCommand : DataCommand<IDataGameObjectContainer>
    {
        public static void Process(IDataGameObjectContainer parent, DataGameObject child, int childIndex = -1)
        {
            parent ??= child.GetParentInHierarchy<DataScene>();
            if (childIndex == -1)
                childIndex = parent.GetCollection().Count;
            var currentParent = child.GetDataGameObjectParent();
            var prevChildIndex = 0;
            if (currentParent != null)
                prevChildIndex = currentParent.GetCollection().IndexOf(child);
            
            new SetDataGameObjectParentCommand(parent, currentParent, child, childIndex, prevChildIndex).Process();
        }

        public readonly DataGameObject Child;
        public readonly int ChildIndexPrev;
        public readonly int ChildIndexNew;
        public readonly IDataGameObjectContainer ParentNew;
        public readonly IDataGameObjectContainer ParentPrev;
        
        private SetDataGameObjectParentCommand(
            IDataGameObjectContainer parentNew, 
            IDataGameObjectContainer parentPrev, 
            DataGameObject child,
            int childIndexNew,
            int childIndexPrev) : base(parentNew)
        {
            ChildIndexNew = childIndexNew;
            ChildIndexPrev = childIndexPrev;
            Child = child;
            ParentNew = parentNew;
            ParentPrev = parentPrev;
        }

        public override ICommand CreateUndo()
        {
            return new SetDataGameObjectParentCommand(ParentPrev, ParentNew, Child, ChildIndexPrev, ChildIndexNew);
        }

        protected override void ProcessInternal()
        {
            if (Child.IsFromPrefab && Child.PrefabId.IsNullOrEmpty)
                throw new Exception($"Cannot set parent of type {nameof(DataGameObject)} created from prefab");
    
            if (ParentNew is DataGameObject dataGameObject)
                if (dataGameObject.IsFromPrefab && dataGameObject.PrefabId.IsNullOrEmpty) 
                    throw new Exception($"Cannot set parent of type {nameof(DataGameObject)} created from prefab");
            
            Assert.IsNotNull(ParentNew);

            var index = ChildIndexNew;
            
            ParentPrev?.GetCollection().Remove(Child);
            if (ParentPrev == ParentNew && ChildIndexNew > ChildIndexPrev)
                index--;
            
            ParentNew.GetCollection().Insert(index, Child);
        }
    }
}

