using TMPro;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
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

