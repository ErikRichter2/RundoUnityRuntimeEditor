using Rundo.Core.Data;

namespace Rundo.RuntimeEditor.Data
{
    public interface IDataComponentReference : IGuid
    {
        void SetDataGameObjectId(DataGameObjectId dataGameObjectId);
        DataGameObject GetDataGameObject(IDataGameObjectFinder dataGameObjectFinder);
    }
}

