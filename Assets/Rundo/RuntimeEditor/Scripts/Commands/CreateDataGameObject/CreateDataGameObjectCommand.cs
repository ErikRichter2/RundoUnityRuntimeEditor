using System;
using Rundo.Core.Commands;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data;

namespace Rundo.RuntimeEditor.Commands
{
    public class CreateDataGameObjectCommand : DataCommand<DataScene>
    {
        public static void Process(DataScene dataScene, DataGameObject dataGameObject, IDataGameObjectContainer parent = null)
        {
            if (dataGameObject == null)
                return;

            parent ??= dataScene;
            
            new CreateDataGameObjectCommand(dataScene, dataGameObject, parent).Process();
        }

        public readonly DataGameObject DataGameObject;
        public readonly IDataGameObjectContainer Parent;
        
        public CreateDataGameObjectCommand(DataScene dataScene, DataGameObject dataGameObject, IDataGameObjectContainer parent) : base(dataScene)
        {
            DataGameObject = dataGameObject;
            Parent = parent;
        }

        public override ICommand CreateUndo()
        {
            return new DestroyDataGameObjectCommand(Data, DataGameObject, Parent);
        }

        protected override void ProcessInternal()
        {
            Data.CheckObjectIdsBeforeAdd(Parent, DataGameObject);
            Parent.GetCollection().Add(DataGameObject);
            DataGameObject.IsDestroyed = false;
            DataGameObject.SetParent(Parent.GetCollection());
        }
    }
}

