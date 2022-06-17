using System.Collections.Generic;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    [RequireComponent(typeof(ProjectWindowBehaviour))]
    public abstract class ProjectWindowBaseDataProviderBehaviour : EditorBaseBehaviour
    {
        public abstract List<ProjectItemMetaData> GetData();
    }
}
