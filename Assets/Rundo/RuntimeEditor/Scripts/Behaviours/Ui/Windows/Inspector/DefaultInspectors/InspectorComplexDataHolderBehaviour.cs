using TMPro;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    /// <summary>
    /// Wrapper over complex objects (classes/structs) - shows expand button and a name
    /// </summary>
    public class InspectorComplexDataHolderBehaviour : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private ExpandCollapseButtonBehaviour _expandCollapseButton;
        [SerializeField] private Transform _content;

        public Transform Content => _content;

        public void SetLabel(string label)
        {
            _label.text = label;
        }
    }
}

