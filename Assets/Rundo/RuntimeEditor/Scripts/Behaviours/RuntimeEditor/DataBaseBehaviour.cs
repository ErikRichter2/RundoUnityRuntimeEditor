using System.Collections.Generic;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data.UiDataMapper;
using Rundo.RuntimeEditor.Tools;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// For UI behaviours that are drawing a data set. Listens to data changes and redraws the UI.
    /// </summary>
    public abstract class DataBaseBehaviour : EditorBaseBehaviour
    {
        public readonly UiDataMapper UiDataMapper = new UiDataMapper();

        public DataHandler DataHandler => UiDataMapper.DataHandler;

        private bool _redraw;
        private bool _onDataSet;
    
        private void Start()
        {
            if (GetUiDataMapperDefaultContent != null)
                UiDataMapper.SetUiElementsContent(GetUiDataMapperDefaultContent);
            
            StartInternal();
            MapUi();
            Redraw();
            GetComponentInParent<CanvasRebuilderBehaviour>()?.Rebuild();
        }

        private void Update()
        {
            UpdateInternal();

            if (_onDataSet)
            {
                _onDataSet = false;
                OnDataSetInternal();
                _redraw = true;
            }

            if (_redraw)
            {
                _redraw = false;
                UiDataMapper.Redraw();
                RedrawInternal();
            }
        }

        protected override void OnDestroyInternal()
        {
            base.OnDestroyInternal();
            UiDataMapper.Clear();
        }

        protected void SetDataRaw(List<object> data)
        {
            Assert.IsNotNull(UiDataMapper.DataHandler);
            
            ClearData();
            UiDataMapper.DataHandler.SetRootDataList(data);
            _onDataSet = true;
        }

        public void SetData(DataHandler data)
        {
            if (IsDataSame(data))
                return;

            UiDataMapper.SetDataHandler(data);
            UiDataMapper.OnRootDataChange(Redraw);

            _onDataSet = true;
        }

        protected bool HasData()
        {
            return UiDataMapper.DataHandler != null && UiDataMapper.DataHandler.GetRootData().Count > 0;
        }

        private void ClearData()
        {
            if (HasData())
                UiDataMapper.DataHandler.ClearRootData();
        }

        protected List<object> GetData()
        {
            return HasData() ? UiDataMapper.DataHandler.GetRootData() : new List<object>();
        }

        protected void Redraw()
        {
            _redraw = true;
        }

        private bool IsDataSame(DataHandler data)
        {
            if (UiDataMapper.DataHandler == null)
                return false;
            
            if (UiDataMapper.DataHandler.GetRootData().Count != data.GetRootData().Count)
                return false;
            
            foreach (var it in data.GetRootData())
                if (UiDataMapper.DataHandler.GetRootData().Contains(it) == false)
                    return false;

            foreach (var it in UiDataMapper.DataHandler.GetRootData())
                if (data.GetRootData().Contains(it) == false)
                    return false;

            return true;
        }


        protected abstract void MapUi();

        protected virtual void StartInternal() {}
        protected virtual void UpdateInternal() {}
        protected virtual void OnDataSetInternal() {}
        protected virtual void RedrawInternal() {}

        protected virtual Transform GetUiDataMapperDefaultContent => null;
    }
}



