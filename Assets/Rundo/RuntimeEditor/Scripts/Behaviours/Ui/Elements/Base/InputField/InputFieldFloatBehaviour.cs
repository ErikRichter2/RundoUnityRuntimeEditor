using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    [RequireComponent(typeof(InputFieldBehaviour))]
    public class InputFieldFloatBehaviour : InputFieldGenericValueBehaviour<float>, IUiDataMapperElementValueChangeableByCursorDragBehaviour
    {
        public override bool IsMouseDragAvailable => true;

        protected override float ValueFromString(string value)
        {
            return float.Parse(value);
        }

        protected override string ValueToString(float value)
        {
            return value.ToString();
        }

        protected override float GetMouseDragValue(float delta)
        {
            var stepDelta = 0.1f;

            var mult = Mathf.Max(1, Mathf.Abs(delta) / 5f);
            return Value + delta * stepDelta * mult;
        }

    }
}


