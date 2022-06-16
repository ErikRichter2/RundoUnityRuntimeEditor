using System.Collections.Generic;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    [RequireComponent(typeof(PrefabsWindowBehaviour))]
    public abstract class ProjectWindowBaseDataProvider : EditorBaseBehaviour
    {
        public abstract List<ProjectItemMetaData> GetData();
    }
}
