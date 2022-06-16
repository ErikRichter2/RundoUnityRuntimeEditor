using System;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// This game object handles a mouse-over/selected state (outline).
    /// </summary>
    public class EditorRaycastHitColliderHandlerBehaviour : MonoBehaviour
    {
        public enum SelectionStateEnum
        {
            None,
            Temporary,
            Selected
        }

        private Outline _quickOutline;
        private SelectionStateEnum _selectionState;

        public SelectionStateEnum SelectionState
        {
            get => _selectionState;
            set
            {
                if (_selectionState == value)
                    return;

                _selectionState = value;
                RefreshOutline();
            }
        }

        private void Start()
        {
            _quickOutline = gameObject.AddComponent<Outline>();
            _quickOutline.OutlineMode = Outline.Mode.OutlineAll;
            _quickOutline.OutlineWidth = 5f;
            _quickOutline.enabled = false;
            RefreshOutline();
        }

        private void RefreshOutline()
        {
            if (_quickOutline == null)
                return;
            
            switch (_selectionState)
            {
                case SelectionStateEnum.None:
                    _quickOutline.enabled = false;
                    break;
                case SelectionStateEnum.Temporary:
                    if (_quickOutline.OutlineColor != Color.yellow)
                        _quickOutline.OutlineColor = Color.yellow;
                    if (_quickOutline.enabled == false)
                        _quickOutline.enabled = true;
                    break;
                case SelectionStateEnum.Selected:
                    if (_quickOutline.OutlineColor != Color.green)
                        _quickOutline.OutlineColor = Color.green;
                    if (_quickOutline.enabled == false)
                        _quickOutline.enabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_selectionState), _selectionState, null);
            }
        }

    }
}



