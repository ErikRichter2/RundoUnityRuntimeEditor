using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    public class ProjectItemsSearchFilterBehaviour : SearchFilterBehaviour<ProjectItemMetaData>
    {
        protected override bool FilterFunction(string literal, ProjectItemMetaData data)
        {
            if (data.GameObject != null)
                return data.GameObject.name.ToLower().Contains(literal);
            if (string.IsNullOrEmpty(data.FolderName) == false)
                return data.FolderName.ToLower().Contains(literal);
            return false;
        }
    }

    public struct ProjectItemMetaData
    {
        public GameObject GameObject;
        public string FolderName;
    }
}
