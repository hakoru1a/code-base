using Contracts.Common.Interface;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Common
{
    public class SerializeService : ISerializeService
    {
        private readonly JsonSerializerOptions _defaultOptions;

        public SerializeService()
        {
            _defaultOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };
        }

        public T Deserialize<T>(string value)
        {
            if (string.IsNullOrEmpty(value))
                return default(T)!;

            try
            {
                return JsonSerializer.Deserialize<T>(value, _defaultOptions)!;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to deserialize JSON: {ex.Message}", ex);
            }
        }

        public string Serialize<T>(T obj)
        {
            if (obj == null)
                return string.Empty;

            return JsonSerializer.Serialize(obj, _defaultOptions);
        }

        public string Serialize<T>(T obj, Type type)
        {
            if (obj == null)
                return string.Empty;

            return JsonSerializer.Serialize(obj, type, _defaultOptions);
        }
    }
}
