using Rundo.Core.Data;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    public abstract class InspectorWindowElementBehaviour : DataBaseBehaviour, IInspectorWindowElementBehaviour
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

