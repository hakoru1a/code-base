using System.Net;
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
        public ApiResult(string? message = null, HttpStatusCode status = HttpStatusCode.OK)
        {
            Message = message;
            Status = (int)status;
            Success = Status >= 200 && Status < 300;
            Timestamp = DateTime.UtcNow;
        }

        public ApiResult(T data, string? message = null, HttpStatusCode status = HttpStatusCode.OK)
        {
            Data = data;
            Message = message;
            Status = (int)status;
            Success = Status >= 200 && Status < 300;
            Timestamp = DateTime.UtcNow;
        }

        public ApiResult(T data, string message, object metadata, HttpStatusCode status = HttpStatusCode.OK)
        {
            Data = data;
            Message = message;
            Metadata = metadata;
            Status = (int)status;
            Success = Status >= 200 && Status < 300;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Indicates whether the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Response data
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Additional metadata (pagination, etc.)
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Metadata { get; set; }

        /// <summary>
        /// Timestamp of the response
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
