using System.Collections.Generic;
using Rundo.Core.Data;
using UnityEngine.Assertions;

namespace Rundo.UnitTests
{
    public class DemoScriptPolymorphism
    {
        private class MainData : BaseData
        {
            public List<ChildDataBase> Children = new List<ChildDataBase>();
        }

        private class ChildDataBase : BaseData {}
    
        [DataTypeId("dee1944d-cc72-477c-b839-d17af124c753")]
        private class ChildData1 : ChildDataBase {}
    
        [DataTypeId("1255dbfb-56da-4bfe-940f-11aa4c314410")]
        private class ChildData2 : ChildDataBase {}
    
        public static void Run()
        {
            MainData mainData = RundoEngine.DataFactory.Instantiate<MainData>();
            mainData.Children.Add(mainData.Instantiate<ChildData1>());
            mainData.Children.Add(mainData.Instantiate<ChildData2>());
        
            // serialize to json
            string serialized = RundoEngine.DataSerializer.SerializeObject(mainData);
        
            // deserialize from json
            MainData mainDataCopy = RundoEngine.DataSerializer.DeserializeObject<MainData>(serialized);

            Assert.IsTrue(mainDataCopy.Children[0] is ChildData1);
            Assert.IsTrue(mainDataCopy.Children[1] is ChildData2);
        }
    }
}


