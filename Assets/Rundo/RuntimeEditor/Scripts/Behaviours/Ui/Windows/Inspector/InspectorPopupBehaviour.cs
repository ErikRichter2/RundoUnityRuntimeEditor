using Rundo.Core.Data;
using Rundo.RuntimeEditor.Commands;
using Rundo.Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class InspectorPopupBehaviour : DataBaseBehaviour
    {
        [SerializeField] private GameObject _mainContent;
        [SerializeField] private Transform _dynamicContent;
        [SerializeField] private Button _closeBtn;

        protected override void MapUi()
        {
            RegisterUiEvent<SelectionBehaviour.SelectObjectEvent>(OnSelectionChanged);
            RegisterUiEvent<SelectionBehaviour.UnselectObjectEvent>(OnSelectionChanged);

            _mainContent.SetActive(false);
            
            _closeBtn.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                DispatchUiEvent(new EditorUiBehaviour.HideWindowEvent{Window = GetType()});
            });

            gameObject.AddComponent<CssBehaviour>().SetValue(CssPropertyEnum.LabelWidth, 200f);

            SetData(new DataHandler(CommandProcessor));
        }

        private void OnSelectionChanged()
        {
            SetDataRaw(RuntimeEditorController.SelectionBehaviour.GetSelectionData());
        }

        protected override void OnDataSetInternal()
        {
            base.OnDataSetInternal();
            Rebuild();
        }

        private void ClearInspectors()
        {
            foreach (Transform child in _dynamicContent.transform)
                Destroy(child.gameObject);
        }

        protected override void RedrawInternal()
        {
            _mainContent.SetActive(HasData());
        }
        
        private void Rebuild()
        {
            ClearInspectors();

            if (HasData() == false)
                return;
            
            UiElementsFactory.DrawInspector(UiDataMapper.DataHandler, _dynamicContent);
            /*
            UiElementsFactory.InstantiateInspectorPrefab(
                UiDataMapper.DataHandler.GetDataType(), 
                _dynamicContent, false)?.SetData(UiDataMapper.DataHandler.Copy(), "");*/
        }

    }    
}



