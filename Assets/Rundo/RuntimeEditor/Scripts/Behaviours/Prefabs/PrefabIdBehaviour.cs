using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// To provide a link between data and a unity prefab. Serialized data keeps this _guid value so it knows which
    /// prefab should be instantiated.
    /// Each prefab that should be visible in the runtime editor should have this behaviour added.
    /// </summary>
    public class PrefabIdBehaviour : MonoBehaviour
    {
        [SerializeField] private string _guid = TGuid<object>.Create().ToStringRawValue();
        [SerializeField] private bool _hideInPrefabWindow;

        public bool HideInPrefabWindow => _hideInPrefabWindow;

        public TGuid<TPrefabId> Guid => TGuid<TPrefabId>.Create(_guid);
    }
}
