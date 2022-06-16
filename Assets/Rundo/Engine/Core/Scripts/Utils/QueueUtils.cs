using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rundo.Core.Utils
{
    public static class QueueUtils
    {
        public static void EnqueueGameObjectChildren(Queue<GameObject> queue, GameObject go)
        {
            foreach (Transform child in go.transform)
                queue.Enqueue(child.gameObject);
        }
        
        public static void EnqueueList<T>(Queue<T> queue, IEnumerable list)
        {
            if (list == null)
                return;
            foreach (var it in list)
                queue.Enqueue((T)it);
        }
    }
}
