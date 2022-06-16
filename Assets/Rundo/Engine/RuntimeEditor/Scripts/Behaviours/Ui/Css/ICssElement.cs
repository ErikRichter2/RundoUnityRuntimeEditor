namespace Rundo.RuntimeEditor.Behaviours
{
    public interface ICssElement
    {
        CssBehaviour GetOrCreateCss();
        void InitDefaultCssValues(CssBehaviour cssBehaviour);
        void UpdateCss(CssBehaviour cssBehaviour);
    }
}


