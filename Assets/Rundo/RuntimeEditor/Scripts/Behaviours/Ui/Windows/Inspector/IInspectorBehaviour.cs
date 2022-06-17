using Rundo.Core.Data;
using UnityEngine;

namespace Rundo.Ui
{
    public interface IInspectorBehaviour
    {
        GameObject GameObject { get; }
        string Label { get; set; }
        void SetData(DataHandler dataHandler, string label);
    }
}

