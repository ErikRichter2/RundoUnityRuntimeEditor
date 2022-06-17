using System;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    public class ContextMenuItemData<T> : IContextMenuItemData
    {
        public T Data;
        public string Name { get; set; }
        public GameObject GameObject { get; set; }
        public Action<T> Callback;
        public bool Disabled;
    }
}

