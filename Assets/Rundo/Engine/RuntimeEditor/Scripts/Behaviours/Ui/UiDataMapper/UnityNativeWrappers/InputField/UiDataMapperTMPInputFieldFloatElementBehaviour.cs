namespace Rundo.RuntimeEditor.Behaviours
{
    public class UiDataMapperTMPInputFieldFloatElementBehaviour : UiDataMapperTMPInputFieldElementBehaviour<float>
    {
        protected override float FromStringToValue(string value) { return float.Parse(value); }
        protected override string FromValueToString(float value) { return value.ToString(); }
    }
}
