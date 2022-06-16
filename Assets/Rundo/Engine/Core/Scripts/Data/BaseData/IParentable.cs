namespace Rundo.Core.Data
{
    public interface IParentable
    {
        IParentable Parent { get; }
        void SetParent(IParentable parent);
        T GetParentInHierarchy<T>();
    }
}

