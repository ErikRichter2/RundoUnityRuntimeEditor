namespace Rundo.RuntimeEditor.Behaviours
{
    public class UiDataMapperTMPInputFieldStringElementBehaviour : UiDataMapperTMPInputFieldElementBehaviour<string>
    {
        protected override string FromStringToValue(string value) { return value; }
        protected override string FromValueToString(string value) { return value; }
    }
}
