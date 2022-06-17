using Rundo.Core.Data;
using Rundo.RuntimeEditor.Factory;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    public class InspectorWindowBehaviour : DataBaseBehaviour
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

        protected override void RedrawInternal()
        {
            _mainContent.SetActive(HasData());
        }
        
        private void Rebuild()
        {
            foreach (Transform child in _dynamicContent.transform)
                Destroy(child.gameObject);

            InspectorFactory.DrawInspector(UiDataMapper.DataHandler, _dynamicContent);
        }

    }    
}



