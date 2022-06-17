using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rundo.Core.Data
{
    public interface IDataSerializerPopulateHandler
    {
        void Populate(string data, JsonSerializerSettings jsonSerializerSettings);
        void Populate(JObject jObject, JsonSerializer serializer);
    }
}