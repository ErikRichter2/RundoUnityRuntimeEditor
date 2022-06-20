using Newtonsoft.Json;
using Rundo.Core.Data;
using Rundo.Core.Events;
using Rundo.RuntimeEditor.Data;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    public abstract class DataComponentMonoBehaviour : MonoBehaviour, IParentable, ICustomDataDispatcher
    {
        [JsonIgnore]
        public IParentable Parent { get; set; }

        [JsonIgnore] public bool IsDataOnlyComponent = true;
        [JsonIgnore] public DataComponent DataComponent;
        [JsonIgnore] public DataGameObject DataGameObject;

        public void SetParent(IParentable parent)
        {
            Parent = parent;
        }

        public T GetParentInHierarchy<T>()
        {
            if (this is T t1)
                return t1;
            if (Parent is T t2)
                return t2;
            if (Parent == default)
                return default;
            return Parent.GetParentInHierarchy<T>() ?? default;
        }
        
        public virtual void OnFromBehaviourToData(DataComponent dataComponent)
        {
        }

        public virtual void OnFromDataToBehaviour(DataComponent dataComponent)
        {
        }

        public void DispatchEvent(IEventSystem eventDispatcher, bool wasProcessed)
        {
            eventDispatcher.Dispatch(DataComponent, wasProcessed);
        }
    }

}

