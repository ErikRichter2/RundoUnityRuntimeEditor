using Rundo.Core.Data;
using Rundo.RuntimeEditor.Attributes;
using Rundo.RuntimeEditor.Data;
using Rundo.Ui;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    [CustomInspector(typeof(DataScene), false)]
    public class CustomInspectorDataSceneBehaviour : InspectorBaseBehaviour
    {
        [SerializeField] private Transform _content;

        protected override void MapUi()
        {
            UiDataMapper.SetUiElementsContent(_content);

            UiDataMapper.Create<TGuid<DataScene.TDataSceneId>>("Guid").Bind(nameof(DataScene.DataSceneMetaData), nameof(DataScene.DataSceneMetaData.Guid));
            UiDataMapper.Create<string>("Name").Bind(nameof(DataScene.DataSceneMetaData), nameof(DataScene.DataSceneMetaData.Name));
            UiDataMapper.CreateButton("Delete Scene").OnClick(() =>
            {
                RuntimeEditorController.DeleteScene();
            });
        }
    }    
}



