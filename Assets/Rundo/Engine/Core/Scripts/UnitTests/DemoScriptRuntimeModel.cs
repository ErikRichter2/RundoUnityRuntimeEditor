using Newtonsoft.Json;
using Rundo.Core.Data;
using UnityEngine.Assertions;

namespace Rundo.UnitTests
{
    public class DemoScriptRuntimeModel
    {
       /**
       * Data with implicit model of type SerializedDataModel<DataA>
       */
        private class DataA : BaseData
        {
        }

        /**
        * Data with explicit model of type DataBModel, model is created at the serialized data instantiation process
        */
        private class DataB : BaseData
        {
            public class DataBModel : DataModel<DataB> {}
    
            [JsonIgnore]
            [ExplicitModel]
            // property with the set method instantiates model class at instantiation process
            public DataBModel Model { get; set; }
        }

        /**
     * Data with explicit model of type DataCModel, model is created at the first request (first call of the Model
     * property)
     */
        private class DataC : BaseData
        {
            public class DataCModel : DataModel<DataC> {}

            [JsonIgnore]
            [ExplicitModel]
            // property without the set method instantiates model class at the first request
            public DataCModel Model => (DataCModel)GetOrCreateExplicitModel();
        }

        public static void Run()
        {
            DataA dataA = RundoEngine.DataFactory.Instantiate<DataA>();
            DataB dataB = RundoEngine.DataFactory.Instantiate<DataB>();
            DataC dataC = RundoEngine.DataFactory.Instantiate<DataC>();
        
            Assert.IsTrue(dataA.GetModel<DataA>() is DataModel<DataA>);
            Assert.IsTrue(dataB.GetModel<DataB>() is DataModel<DataB>);
            Assert.IsTrue(dataB.GetModel<DataB>() is DataB.DataBModel);
            Assert.IsTrue(dataC.GetModel<DataC>() is DataModel<DataC>);
            Assert.IsTrue(dataC.GetModel<DataC>() is DataC.DataCModel);
        }
    }

}

