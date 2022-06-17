using Rundo.RuntimeEditor.Data;
using Rundo.Core.Utils;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class DataGameObjectsSearchFilterBehaviour : SearchFilterBehaviour<DataGameObjectTreeHierarchy>
    {
        protected override bool FilterFunction(string literal, DataGameObjectTreeHierarchy data)
        {
            return data.DataGameObject.Name.Contains(literal);
        }
    }
}
