using System.Collections.Generic;
using Rundo.Core.Data;
using UnityEngine.Assertions;

namespace Rundo.UnitTests
{
    public class DemoScriptChildParentHierarchy
    {
        private class MainData : BaseData
        {
            public ChildDataA ChildDataA;
            public DataList<ChildDataB> ChildrenB;

            public override void OnInstantiated()
            {
                base.OnInstantiated();
                ChildrenB = InstantiateList<ChildDataB>();
            }
        }

        private class ChildDataA : BaseData
        {
            public ChildDataB ChildDataB;

            public override void OnInstantiated()
            {
                base.OnInstantiated();

                ChildDataB = Instantiate<ChildDataB>();
            }
        }
        
        private class ChildDataB : BaseData {}
        
        public static void Run()
        {
            MainData mainData = RundoEngine.DataFactory.Instantiate<MainData>();
            
            // using InstantiateSerializedData keeps the child -> parent hierarchy in the runtime

            mainData.ChildDataA = mainData.Instantiate<ChildDataA>();
            mainData.ChildrenB.Add(mainData.Instantiate<ChildDataB>());

            MainData parent = mainData.ChildDataA.GetParentInHierarchy<MainData>();
            Assert.IsNotNull(parent);
            Assert.IsTrue(parent == mainData);

            parent = mainData.ChildDataA.ChildDataB.GetParentInHierarchy<MainData>();
            Assert.IsNotNull(parent);
            Assert.IsTrue(parent == mainData);

            parent = mainData.ChildrenB[0].GetParentInHierarchy<MainData>();
            Assert.IsNotNull(parent);
            Assert.IsTrue(parent == mainData);

            // serialize to json
            string serialized = RundoEngine.DataSerializer.SerializeObject(mainData);
            
            // deserialize from json
            MainData mainDataCopy = RundoEngine.DataSerializer.DeserializeObject<MainData>(serialized);
            
            // deserializing implicitly sets the child -> parent hierarchy
            
            parent = mainDataCopy.ChildDataA.GetParentInHierarchy<MainData>();
            Assert.IsNotNull(parent);
            Assert.IsTrue(parent == mainDataCopy);

            parent = mainDataCopy.ChildDataA.ChildDataB.GetParentInHierarchy<MainData>();
            Assert.IsNotNull(parent);
            Assert.IsTrue(parent == mainDataCopy);

            parent = mainDataCopy.ChildrenB[0].GetParentInHierarchy<MainData>();
            Assert.IsNotNull(parent);
            Assert.IsTrue(parent == mainDataCopy);
        }
    }
    
}
