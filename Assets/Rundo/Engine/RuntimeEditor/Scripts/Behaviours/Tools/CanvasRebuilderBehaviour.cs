using System;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Tools
{
    /// <summary>
    /// Rebuilds the transform (and all children transforms) so the layout groups are always rendered correctly, and
    /// not stuck.
    /// </summary>
    public class CanvasRebuilderBehaviour : MonoBehaviour
    {
        [SerializeField] private bool _rebuildAtStart;
        
        private int _updateFrames;
        private Action _rebuildFinished;

        private void Start()
        {
            if (_rebuildAtStart)
                Rebuild();
        }

        public void Rebuild(Action rebuildFinished = null)
        {
            _rebuildFinished = rebuildFinished;
            if (_updateFrames <= 0)
                _updateFrames = 2;
        }
    
        private void Update()
        {
            if (_updateFrames > 0 && --_updateFrames <= 0)
            {
                if (TryGetComponent<RectTransform>(out var rectTransform))
                {
                    RebuildRecursive(rectTransform);
                    if (gameObject.activeInHierarchy)
                        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                }

                _rebuildFinished?.Invoke();
                _rebuildFinished = null;
            }
        }

        private void RebuildRecursive(RectTransform rectTransform)
        {
            if (rectTransform.gameObject.activeInHierarchy == false)
                return;
            
            foreach (Transform it in rectTransform.transform)
            {
                if (it.TryGetComponent(out rectTransform))
                {
                    RebuildRecursive(rectTransform);
                    if (it.gameObject.activeInHierarchy)
                        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                }
            }
        }
    }
}

