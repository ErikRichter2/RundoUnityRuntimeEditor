using Rundo.Core.Data;
using Rundo.RuntimeEditor.Attributes;
using Rundo.RuntimeEditor.Data;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    [CustomInspector(typeof(DataScene))]
    public class CustomInspectorDataSceneBehaviour : InspectorWindowElementBehaviour
    {
        [SerializeField] private Transform _content;

        protected override void MapUi()
        {
            UiDataMapper.SetUiElementsContent(_content);

            UiDataMapper.CreatePrimitive<TGuid<DataScene.TDataSceneId>>("Guid").Bind(nameof(DataScene.DataSceneMetaData), nameof(DataScene.DataSceneMetaData.Guid));
            UiDataMapper.CreatePrimitive<string>("Name").Bind(nameof(DataScene.DataSceneMetaData), nameof(DataScene.DataSceneMetaData.Name));
            UiDataMapper.CreateButton("Delete Scene").OnClick(() =>
            {
                RuntimeEditorController.DeleteScene();
            });
        }
    }    
}



