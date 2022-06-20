using System.Threading.Tasks;
using Rundo.Core.Commands;
using Rundo.Core.Data;
using Rundo.Core.Events;
using Rundo.Core.Utils;
using Rundo.RuntimeEditor.Data;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// To provide different independent contexts - for example one context is editor-context and other is playmode-context,
    /// both of them have its own command/event system so they never interfere with each other.
    /// </summary>
    public interface IBaseDataProviderBehaviour
    {
        CommandProcessor GetCommandProcessor();
        EventSystem GetUiEventDispatcher();
        DataScene GetDataScene();
        Task<PrefabIdBehaviour> LoadPrefab(TGuid<TPrefabId> prefabId);
        void PostprocessGameObject(GameObject gameObject);
    }
}


