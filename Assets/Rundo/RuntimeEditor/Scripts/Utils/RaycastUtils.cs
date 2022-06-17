using System.Collections.Generic;
using Rundo.RuntimeEditor.Behaviours;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rundo.RuntimeEditor.Utils
{
    public static class RaycastUtils
    {
        public static List<RaycastResult> RaycastUi()
        {
            var res = new List<RaycastResult>();
            
            var pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;
            
            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            foreach (var it in raycastResults)
            {
                if (it.gameObject.GetComponentInParent<IIgnoreRayCast>() == null)
                    res.Add(it);
            }

            return res;
        }
    }
}
