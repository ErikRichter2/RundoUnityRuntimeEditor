using Newtonsoft.Json;
using Rundo.Core.Commands;
using Rundo.Core.Data;
using UnityEngine.Assertions;

namespace Rundo.UnitTests
{
    public class DemoScriptDataManipulation
    {
        /**
     * To allow for undo/redo changes the root Data must implement ICommandProcessorProvider. All SerializedData children
     * will try to get the parent's CommandProcessor
     */
        private class Data : BaseData, ICommandProcessorProvider
        {
            public string StringProp;
            public int IntProp;
        
            // undo/redo system
            [JsonIgnore]
            public ICommandProcessor CommandProcessor { get; } = new CommandProcessor();
        }

        public static void Run()
        {
            Data data = RundoEngine.DataFactory.Instantiate<Data>();

            // set initial data
            data.StringProp = "A";
            data.IntProp = 0;
        
            // make change using Modify command so the record of change is kept in the undo/redo system
            data.GetModel<Data>().Modify(copy =>
            {
                copy.StringProp = "B";
                copy.IntProp = 1;
            });

            Assert.IsTrue(data.StringProp == "B");
            Assert.IsTrue(data.IntProp == 1);

            // revert change
            data.CommandProcessor.Undo();
        
            Assert.IsTrue(data.StringProp == "A");
            Assert.IsTrue(data.IntProp == 0);
        
            // revert revert change
            data.CommandProcessor.Redo();
        
            Assert.IsTrue(data.StringProp == "B");
            Assert.IsTrue(data.IntProp == 1);
        }
    }
}

