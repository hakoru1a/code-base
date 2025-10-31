using System.Text.Json.Serialization;

namespace Shared.SeedWork
{
    /// <summary>
    /// Represents a failed API response
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    public class ApiErrorResult<T> : ApiResult<T>
    {
        public ApiErrorResult()
            : base(ResponseMessages.InternalError)
        {
        }

        public ApiErrorResult(string message)
            : base(message)
        {
        }

        public ApiErrorResult(List<string> errors)
            : base(ResponseMessages.ValidationFailed)
        {
            Errors = errors;
        }

        public ApiErrorResult(string message, List<string> errors)
            : base(message)
        {
            Errors = errors;
        }

        /// <summary>
        /// List of validation or error messages
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Errors { get; set; }
    }
}
