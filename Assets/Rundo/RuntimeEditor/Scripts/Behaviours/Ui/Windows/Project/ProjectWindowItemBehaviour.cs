using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    public class ProjectWindowItemBehaviour : EditorBaseBehaviour
    {
        [SerializeField] private TMP_Text _prefabName;
        [SerializeField] private Button _button;
        [SerializeField] private RawImage _icon;

        public ProjectItemMetaData ProjectItemMetaData { get; private set; }
        
        private void Start()
        {
            _button.onClick.AddListener(() =>
            {
                if (ProjectItemMetaData.GameObject != null)
                {
                    if (ProjectItemMetaData.GameObject.TryGetComponent<PrefabIdBehaviour>(out var prefabIdBehaviour))
                    {
                        var dataGameObject = DataScene.InstantiateDataGameObjectFromPrefab(prefabIdBehaviour);

                        dataGameObject.GetComponent<DataGameObjectBehaviour>().DataComponentPrefab.OverridePrefabComponent = true;
                        dataGameObject.GetComponent<DataTransformBehaviour>().DataComponentPrefab.OverridePrefabComponent = true;

                        RuntimeEditorController.SetMode<PlaceObjectsEditorModeBehaviour>().SetData(dataGameObject);
                    }
                }
            });
        }

        public void SetData(ProjectItemMetaData projectItemMetaData)
        {
            ProjectItemMetaData = projectItemMetaData;

            if (ProjectItemMetaData.GameObject != null)
                _prefabName.text = ProjectItemMetaData.GameObject.name;
            else
                _prefabName.text = ProjectItemMetaData.FolderName;
        }

        public void UpdateScreenshot(Texture2D screenshot)
        {
            _icon.texture = screenshot;
        }
        
    }
}
