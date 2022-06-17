using System;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data.UiDataMapper;
using TMPro;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    public abstract class UiDataMapperTMPInputFieldElementBehaviour<TValue> : UiDataMapperElementBehaviour<TValue>
    {
        protected abstract TValue FromStringToValue(string value);
        protected abstract string FromValueToString(TValue value);

        public override void OnSubmit(Action<UiDataMapperElementValue<TValue>> onSubmit)
        {
            GetComponent<TMP_InputField>().onSubmit.AddListener(value =>
            {
                onSubmit(new UiDataMapperElementValue<TValue>(FromStringToValue(value)));
            });
        }

        public override void SetValue(TValue value)
        {
            GetComponent<TMP_InputField>().SetTextWithoutNotify(FromValueToString(value));
        }

        public override void SetUndefValue()
        {
            GetComponent<TMP_InputField>().SetTextWithoutNotify("--");
        }
        
    }
}
