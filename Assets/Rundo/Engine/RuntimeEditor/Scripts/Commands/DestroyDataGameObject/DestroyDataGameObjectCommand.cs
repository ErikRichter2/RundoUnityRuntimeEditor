using Rundo.Core.Commands;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data;

namespace Rundo.RuntimeEditor.Commands
{
    public class DestroyDataGameObjectCommand : DataCommand<DataScene>
    {
        public static void Process(DataScene dataScene, DataGameObject dataGameObject)
        {
            if (dataGameObject == null)
                return;
            if (dataGameObject.IsDestroyed)
                return;

            new DestroyDataGameObjectCommand(dataScene, dataGameObject, dataGameObject.Parent.GetParentInHierarchy<IDataGameObjectContainer>()).Process();
        }

        private readonly IDataGameObjectContainer _parent;
        public readonly DataGameObject DataGameObject;
        
        public DestroyDataGameObjectCommand(DataScene dataScene, DataGameObject dataGameObject, IDataGameObjectContainer parent) : base(dataScene)
        {
            DataGameObject = dataGameObject;
            _parent = parent;
        }

        public override ICommand CreateUndo()
        {
            return new CreateDataGameObjectCommand(Data, DataGameObject, _parent);
        }

        protected override void ProcessInternal()
        {
            if (DataGameObject.IsDestroyed)
                return;

            _parent.GetCollection().Remove(DataGameObject);
            DataGameObject.IsDestroyed = true;
            DataGameObject.SetParent(null);
        }
    }
}

