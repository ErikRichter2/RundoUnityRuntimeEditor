using System;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data;

namespace Rundo.RuntimeEditor.Behaviours
{
    public interface IObjectPickerBehaviour
    {
        IDataComponentReference Value { get; }
        Type GetReferenceType();
        void SubmitValue(DataGameObject dataGameObject);
        string Label { get; }
    }
}


