using System.Text.Json.Serialization;

namespace Shared.SeedWork
{
    /// <summary>
    /// Standard API response wrapper
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    public class ApiResult<T>
    {
        [JsonConstructor]
        public ApiResult(string message = null)
        {
            Message = message;
            Timestamp = DateTime.UtcNow;
        }

        public ApiResult(T data, string message = null)
        {
            Data = data;
            Message = message;
            Timestamp = DateTime.UtcNow;
        }

        public ApiResult(T data, string message, object metadata)
        {
            Data = data;
            Message = message;
            Metadata = metadata;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Response data
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Additional metadata (pagination, etc.)
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object Metadata { get; set; }

        /// <summary>
        /// Timestamp of the response
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
