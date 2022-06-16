using System;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data.UiDataMapper;
using TMPro;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class UiDataMapperDropDownElementBehaviour : UiDataMapperElementBehaviour<int>, ICustomUiDataMapper
    {
        public override int Value => GetComponent<TMP_Dropdown>().value;

        public Type GetDataMapperType()
        {
            return typeof(UiDataMapperElementInstance<>).MakeGenericType(new Type[] { typeof(int) });
        }

        public override void OnSubmit(Action<UiDataMapperElementValue<int>> onSubmit)
        {
            GetComponent<TMP_Dropdown>().onValueChanged.AddListener(value => { onSubmit(new UiDataMapperElementValue<int>(value)); });
        }

        public override void SetValue(int value)
        {
            IsUndefValue = false;
            GetComponent<TMP_Dropdown>().SetValueWithoutNotify(value);
        }

        public override void SetUndefValue()
        {
            IsUndefValue = true;
            GetComponent<TMP_Dropdown>().SetValueWithoutNotify(0);
        }
        
    }
}
