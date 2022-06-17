using Rundo.RuntimeEditor.Data;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    public class HierarchyWindowItemSeparatorBehaviour : HierarchyWindowItemBaseBehaviour
    {
        protected override void SetDataInternal() {}
        
        protected override void ProcessDragDrop(DataGameObject dataGameObject)
        {
            // last
            if (DataGameObject == null)
            {
                dataGameObject.SetDataGameObjectParent(DataScene);
            }
            else
            {
                var index = DataGameObject.GetDataGameObjectParent().GetCollection()
                    .IndexOf(DataGameObject);
                dataGameObject.SetDataGameObjectParent(DataGameObject.GetDataGameObjectParent(), index);
            }
        }
    }
}
