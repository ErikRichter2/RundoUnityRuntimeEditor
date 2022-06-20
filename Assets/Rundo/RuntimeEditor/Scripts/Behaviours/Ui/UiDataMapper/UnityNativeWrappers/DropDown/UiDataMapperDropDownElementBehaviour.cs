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

        protected override void SetValueInternal(int value)
        {
            GetComponent<TMP_Dropdown>().SetValueWithoutNotify(value);
        }

        protected override void SetUndefinedValue()
        {
            GetComponent<TMP_Dropdown>().SetValueWithoutNotify(0);
        }
        
    }
}
