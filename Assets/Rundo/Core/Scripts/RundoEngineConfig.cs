using Rundo.Core.Data;

namespace Rundo
{
    public class RundoEngineConfig
    {
        public DataFactory DataFactory = new DataFactory();
        public JsonDataSerializer DataSerializer = new JsonDataSerializer();
        public ReflectionService ReflectionService = new ReflectionService();
    }
}

