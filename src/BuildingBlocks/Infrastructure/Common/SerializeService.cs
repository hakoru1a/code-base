using Contracts.Common.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Infrastructure.Common
{
    public class SerializeService : ISerializeService
    {
        private readonly JsonSerializerSettings _defaultSettings;

        public SerializeService()
        {
            _defaultSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                }
            };
        }

        public T? Deserialize<T>(string value)
        {
            if (string.IsNullOrEmpty(value))
                return default;

            return JsonConvert.DeserializeObject<T>(value, _defaultSettings);
        }

        public string? Serialize<T>(T obj)
        {
            if (obj == null)
                return null;

            return JsonConvert.SerializeObject(obj, _defaultSettings);
        }

        public string? Serialize<T>(T obj, Type type)
        {
            if (obj == null)
                return null;

            return JsonConvert.SerializeObject(obj, type, _defaultSettings);
        }
    }
}
