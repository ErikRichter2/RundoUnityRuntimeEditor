using System;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    [RequireComponent(typeof(InputFieldBehaviour))]
    public class InputFieldFloatBehaviour : InputFieldGenericValueBehaviour<float>, IUiDataMapperElementValueChangeableByCursorDragBehaviour
    {
        public override bool IsMouseDragAvailable => true;

        public override float Value
        {
            get
            {
                if (IsUndefValue)
                    return 0f;
                return float.Parse(GetComponent<InputFieldBehaviour>().Text);
            }
        }

        protected override float GetMouseDragValue(float delta)
        {
            var stepDelta = 0.1f;
            //if (InputFieldBehaviour.IsStepDelta)
            //    stepDelta = InputFieldBehaviour.StepDelta;

            var mult = Mathf.Max(1, Mathf.Abs(delta) / 5f);
            return Value + delta * stepDelta * mult;
        }

    }
}


