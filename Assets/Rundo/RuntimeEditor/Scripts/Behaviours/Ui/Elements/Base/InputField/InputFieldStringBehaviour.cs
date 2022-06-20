using System;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    [RequireComponent(typeof(InputFieldBehaviour))]
    public class InputFieldStringBehaviour : InputFieldGenericValueBehaviour<string>
    {
        protected override string ValueFromString(string value)
        {
            return value;
        }

        protected override string ValueToString(string value)
        {
            return value;
        }

        protected override string GetMouseDragValue(float delta)
        {
            throw new NotImplementedException();
        }
    }
}


