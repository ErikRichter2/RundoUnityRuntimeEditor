using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    [RequireComponent(typeof(InputFieldBehaviour))]
    public class InputFieldIntBehaviour : InputFieldGenericValueBehaviour<int>, IUiDataMapperElementValueChangeableByCursorDragBehaviour
    {
        public override bool IsMouseDragAvailable => true;

        protected override int ValueFromString(string value)
        {
            return int.Parse(value);
        }

        protected override string ValueToString(int value)
        {
            return value.ToString();
        }

        protected override int GetMouseDragValue(float delta)
        {
            var stepDelta = 0.1f;
            
            var mult = Mathf.Max(1, Mathf.Abs(delta) / 5f);
            return Value + (int)(delta * stepDelta * mult);
        }
    }
}


