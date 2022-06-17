using UnityEngine;

namespace Rundo.UnitTests
{
    public class UnitTests : MonoBehaviour
    {
        private void Start()
        {
            // polymorphism
            DemoScriptPolymorphism.Run();
        
            // child -> parent hierarchy
            DemoScriptChildParentHierarchy.Run();
        
            // runtime implicit/explicit model
            DemoScriptRuntimeModel.Run();
        
            // data manipulation with undo/redo system
            DemoScriptDataManipulation.Run();
        }
    }
}


