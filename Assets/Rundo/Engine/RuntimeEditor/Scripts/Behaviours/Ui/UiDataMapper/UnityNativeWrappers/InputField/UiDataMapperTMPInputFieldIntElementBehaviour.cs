namespace Rundo.RuntimeEditor.Behaviours
{
    public class UiDataMapperTMPInputFieldIntElementBehaviour : UiDataMapperTMPInputFieldElementBehaviour<int>
    {
        protected override int FromStringToValue(string value) { return int.Parse(value); }
        protected override string FromValueToString(int value) { return value.ToString(); }
    }
}
