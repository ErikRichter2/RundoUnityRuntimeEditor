using System;
using Rundo.Core.Data;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    [RequireComponent(typeof(InputFieldBehaviour))]
    public class InputFieldStringBehaviour : InputFieldGenericValueBehaviour<string>
    {
        public override string Value
        {
            get
            {
                if (IsUndefValue)
                    return "--";
                return GetComponent<InputFieldBehaviour>().Text;
            }
        }

        protected override string GetMouseDragValue(float delta)
        {
            throw new NotImplementedException();
        }
        
        /*        
        public override void SetUndefValue()
        {
            IsUndefValue = true;
            GetComponent<InputFieldBehaviour>().SetUndefValue();
        }

        public override void OnSubmit(Action<UiDataMapperElementValue<string>> onSubmit)
        {
            GetComponent<InputFieldBehaviour>().OnSubmit(onSubmit.Invoke);
        }

        public override void SetValue(string value)
        {
            IsUndefValue = false;
            GetComponent<InputFieldBehaviour>().Text = value;
        }

        public override string Value => GetComponent<InputFieldBehaviour>().Text;
        */
    }
}


