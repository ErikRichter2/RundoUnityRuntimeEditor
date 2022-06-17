using Rundo.Core.Data;
using Rundo.RuntimeEditor.Behaviours;
using UnityEngine;

namespace Rundo.Ui
{
    public abstract class InspectorBaseBehaviour : DataBaseBehaviour, IInspectorBehaviour
    {
        public GameObject GameObject => gameObject;
        public virtual string Label { get; set; }

        public void SetData(DataHandler dataHandler, string label)
        {
            Label = label;
            SetData(dataHandler);
        }
    }
}

