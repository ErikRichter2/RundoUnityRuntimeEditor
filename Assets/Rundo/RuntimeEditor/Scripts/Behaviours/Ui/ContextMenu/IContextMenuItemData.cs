using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    public interface IContextMenuItemData
    {
        string Name { get; }
        GameObject GameObject { get; }
    }
}

