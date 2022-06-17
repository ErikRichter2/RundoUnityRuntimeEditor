using Rundo.Core.Data;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    public interface IInspectorWindowElementBehaviour
    {
        GameObject GameObject { get; }
        string Label { get; set; }
        void SetData(DataHandler dataHandler, string label);
    }
}

