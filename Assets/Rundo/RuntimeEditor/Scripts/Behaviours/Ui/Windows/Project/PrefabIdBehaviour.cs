using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data;
using UnityEngine;

namespace Rundo.Core.Utils
{
    public class PrefabIdBehaviour : MonoBehaviour
    {
        [SerializeField] private string _guid = TGuid<object>.Create().ToStringRawValue();
        [SerializeField] private bool _hideInPrefabWindow;

        public bool HideInPrefabWindow => _hideInPrefabWindow;

        public TGuid<TPrefabId> Guid => TGuid<TPrefabId>.Create(_guid);
    }
}
