using System.Collections.Generic;
using Rundo.Core.Data;
using Rundo.RuntimeEditor.Data;
using Rundo.Ui;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    [DataComponent]
    [DataTypeId("DebugTestComponent")]
    public class DebugTestComponentBehaviour : DataComponentMonoBehaviour
    {
        public enum DebugTestComponentTestEnum
        {
            Value1,
            Value2,
            Value3,
        }

        public struct ValueTypeStruct
        {
            public int IntValueType;
            public string StringValueType;
        }

        public class ReferenceTypeClass
        {
            public int IntValueRef;
            public string StringValueRef;
        }

        public class ComplexData
        {
            public ValueTypeStruct ValueTypeComplex;
            public ReferenceTypeClass ReferenceTypeComplex = new ReferenceTypeClass();

            public List<ComplexDataChild> List = new List<ComplexDataChild>()
                { new ComplexDataChild { ChildName = "Child 1" }, new ComplexDataChild { ChildName = "Child 2" } };
        }

        public class ComplexDataChild
        {
            public ValueTypeStruct ChildValueType;
            public string ChildName;

            public List<ComplexDataGrandChild> List = new List<ComplexDataGrandChild>()
                { new ComplexDataGrandChild { ChildName = "Grand Child 1" }, new ComplexDataGrandChild { ChildName = "Grand Child 2" } };
        }

        public class ComplexDataGrandChild
        {
            public ReferenceTypeClass ChildReferenceType = new ReferenceTypeClass();
            public string ChildName;
        }

        public bool Bool;
        public int Int;
        public float Float;
        public string String;
        public Color Color;
        public TDataComponentReference<DebugTestComponentBehaviour> ComponentReference;
        public DebugTestComponentTestEnum Enum;
        public ValueTypeStruct ValueType;
        public ComplexData Complex = new ComplexData();
        public List<int> PrimitiveList = new List<int>() { 1, 2 };
        public List<ValueTypeStruct> ValueTypeList = new List<ValueTypeStruct>()
            { new ValueTypeStruct(), new ValueTypeStruct() };
    }

    
}



