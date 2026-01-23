using System.Net;
using System.Text.Json.Serialization;

namespace Shared.SeedWork
{
    /// <summary>
    /// Represents a failed API response
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    public class ApiErrorResult<T> : ApiResult<T>
    {
        public ApiErrorResult(HttpStatusCode status = HttpStatusCode.InternalServerError)
            : base(ResponseMessages.InternalError, status)
        {
            Success = false;
        }

        public ApiErrorResult(string message, HttpStatusCode status = HttpStatusCode.BadRequest)
            : base(message, status)
        {
            Success = false;
        }

        public ApiErrorResult(List<string> errors, HttpStatusCode status = HttpStatusCode.BadRequest)
            : base(ResponseMessages.ValidationFailed, status)
        {
            Success = false;
            Errors = errors;
        }

        public ApiErrorResult(string message, List<string> errors, HttpStatusCode status = HttpStatusCode.BadRequest)
            : base(message, status)
        {
            Success = false;
            Errors = errors;
        }

        /// <summary>
        /// List of validation or error messages
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Errors { get; set; }
    }
}
