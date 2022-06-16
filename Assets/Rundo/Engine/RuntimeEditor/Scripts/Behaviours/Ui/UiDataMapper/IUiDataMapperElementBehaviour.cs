using System;
using System.Collections.Generic;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data.UiDataMapper;
using Rundo.RuntimeEditor.Behaviours;

namespace Rundo.RuntimeEditor.Behaviours
{
    public interface IUiDataMapperElementBehaviour
    {
    }

    public interface IUiDataMapperDynamicValuesCustomHandler
    {
        
    }
    
    public interface IUiDataMapperElementWithValueBehaviour : IUiDataMapperElementBehaviour
    {
        void SetUndefValue();
        void SetValue(DataHandlerValue dataHandlerValue);
        void SetDynamicValue(object dataValue);
        void OnSubmitDynamicValue(Type expectedDataType, Action<UiDataMapperElementValue<object>> onSubmitDynamicValue);
    }

    public interface ICustomUiDataMapper
    {
        Type GetDataMapperType();
    }

    public interface IUiDataMapperElementBehaviour<TValue> : IUiDataMapperElementWithValueBehaviour
    {
        void OnSubmit(Action<UiDataMapperElementValue<TValue>> onSubmit);
        void SetValue(TValue value);
        //void SetValues(List<TValue> values);
        TValue Value { get; }
    }

    public interface IUiDataMapperElementValueChangeableByCursorDragBehaviour
    {
        void OnRaycasterPointerUp();
        void OnRaycasterPointerDown();
    }

}
