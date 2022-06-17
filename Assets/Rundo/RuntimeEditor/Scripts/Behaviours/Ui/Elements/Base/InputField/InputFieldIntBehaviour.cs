using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    [RequireComponent(typeof(InputFieldBehaviour))]
    public class InputFieldIntBehaviour : InputFieldGenericValueBehaviour<int>, IUiDataMapperElementBehaviour<int>, IUiDataMapperElementValueChangeableByCursorDragBehaviour
    {
        public override bool IsMouseDragAvailable => true;

        public override int Value => int.Parse(GetComponent<InputFieldBehaviour>().Text);

        protected override int GetMouseDragValue(float delta)
        {
            var stepDelta = 0.1f;
            //if (InputFieldBehaviour.IsStepDelta)
            //    stepDelta = InputFieldBehaviour.StepDelta;
            
            var mult = Mathf.Max(1, Mathf.Abs(delta) / 5f);
            return Value + (int)(delta * stepDelta * mult);
        }
    }
}


