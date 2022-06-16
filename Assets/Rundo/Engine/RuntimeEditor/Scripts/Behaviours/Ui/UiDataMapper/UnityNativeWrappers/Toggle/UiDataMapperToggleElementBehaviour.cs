using System;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data.UiDataMapper;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class UiDataMapperToggleElementBehaviour : UiDataMapperElementBehaviour<bool>
    {
        public override bool Value => GetComponent<Toggle>().isOn;

        public override void OnSubmit(Action<UiDataMapperElementValue<bool>> onSubmit)
        {
            GetComponent<Toggle>().onValueChanged.AddListener(value => { onSubmit(new UiDataMapperElementValue<bool>(value)); });
        }

        public override void SetValue(bool value)
        {
            IsUndefValue = false;
            GetComponent<Toggle>().SetIsOnWithoutNotify(value);
        }

        public override void SetUndefValue()
        {
            IsUndefValue = true;
            GetComponent<Toggle>().SetIsOnWithoutNotify(false);
        }
        
    }
}
