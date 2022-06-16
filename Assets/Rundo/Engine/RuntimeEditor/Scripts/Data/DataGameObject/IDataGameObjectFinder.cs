using Rundo.RuntimeEditor.Behaviours;

namespace Rundo.RuntimeEditor.Data
{
    public interface IDataGameObjectFinder
    {
        DataGameObject Find(DataGameObjectId dataGameObjectId);
    }
    
    public interface IDataGameObjectBehaviourFinder
    {
        DataGameObjectBehaviour Find(DataGameObjectId dataGameObjectId);
    }

}

