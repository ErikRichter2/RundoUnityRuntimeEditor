using System.Collections.Generic;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class ProjectWindowResourcesDataProvider : ProjectWindowBaseDataProvider
    {
        private void Start()
        {
        }

        public override List<ProjectItemMetaData> GetData()
        {
            var res = new List<ProjectItemMetaData>();
            
            foreach (var prefab in RuntimeEditor.GetPrefabs())
                if (prefab.HideInPrefabWindow == false)
                    res.Add(new ProjectItemMetaData{GameObject = prefab.gameObject});

            return res;
        }
    }
}
