using System;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data.UiDataMapper;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    public interface IUiDataMapperElementBehaviour
    {
        public GameObject GameObject { get; }
    }

    public interface IUiDataMapperDynamicValuesCustomHandler
    {
        
    }
    
    public interface IUiDataMapperElementWithValueBehaviour : IUiDataMapperElementBehaviour
    {
        void SetValue(DataHandlerValue dataHandlerValue);
        void OnSubmitDynamicValue(Type expectedDataType, Action<UiDataMapperElementValue<object>> onSubmitDynamicValue);
    }

    public interface ICustomUiDataMapper
    {
        Type GetDataMapperType();
    }

    public interface IUiDataMapperElementBehaviour<TValue> : IUiDataMapperElementWithValueBehaviour
    {
        void OnSubmit(Action<UiDataMapperElementValue<TValue>> onSubmit);
    }

    public interface IUiDataMapperElementValueChangeableByCursorDragBehaviour
    {
        bool IsMouseDragAvailable { get; }
        void OnRaycasterPointerUp();
        void OnRaycasterPointerDown();
    }

}
