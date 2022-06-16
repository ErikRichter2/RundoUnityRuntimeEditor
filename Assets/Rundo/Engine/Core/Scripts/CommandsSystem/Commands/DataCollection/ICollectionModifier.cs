using System.Collections;

namespace Rundo.Core.Commands
{
    public interface ICollectionModifierChild<out T> : ICollectionModifier
    {
        T Child { get; }
    }
    
    public interface ICollectionModifierParent<out T> : ICollectionModifier
    {
        T Parent { get; }
    }

    public interface ICollectionModifier : ICommand
    {
        object CollectionOwner { get; }
        IList Collection { get; }
    }
}

