using Rundo.Core.Data;

namespace Rundo.RuntimeEditor.Data
{
    public interface IDataGameObjectContainer : IParentable
    {
        DataList<DataGameObject> GetCollection();
        DataGameObject Find(DataGameObjectId dataGameObjectId);
    }
}

