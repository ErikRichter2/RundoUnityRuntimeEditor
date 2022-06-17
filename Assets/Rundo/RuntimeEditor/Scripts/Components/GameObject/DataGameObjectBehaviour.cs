using Rundo.Core.Data;
using Rundo.Ui;

namespace Rundo.RuntimeEditor.Behaviours
{
    [DataComponent]
    [DataTypeId("7ae53677-b364-4bd0-b906-75c0c4c12492")]
    public class DataGameObjectBehaviour : DataComponentMonoBehaviour
    {
        private string _name = "GameObject";
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                if (IsRuntimeOnlyComponent)
                    gameObject.name = _name;
            }
        }

        private bool _isActive = true;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                if (IsRuntimeOnlyComponent)
                    gameObject.SetActive(_isActive);
            }
        }
    }
}



