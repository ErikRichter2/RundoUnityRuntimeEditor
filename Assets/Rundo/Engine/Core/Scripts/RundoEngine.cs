using Rundo.Core.Data;

namespace Rundo
{
    public static class RundoEngine
    {
        public static DataFactory DataFactory => Config.DataFactory;
        public static JsonDataSerializer DataSerializer => Config.DataSerializer;
        public static ReflectionService ReflectionService => Config.ReflectionService;
        
        public static RundoEngineConfig Config = new RundoEngineConfig();
    }
}

