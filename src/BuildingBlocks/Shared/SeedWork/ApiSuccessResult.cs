using System.Net;
using System.Text.Json.Serialization;

namespace Shared.SeedWork
{
    /// <summary>
    /// Represents a successful API response
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    public class ApiSuccessResult<T> : ApiResult<T>
    {
        [JsonConstructor]
        public ApiSuccessResult(T data)
            : base(data, ResponseMessages.RetrieveSuccess, HttpStatusCode.OK)
        {
            Success = true;
        }

        public ApiSuccessResult(T data, string message, HttpStatusCode status = HttpStatusCode.OK)
            : base(data, message, status)
        {
            Success = true;
        }

        public ApiSuccessResult(T data, string message, object metadata, HttpStatusCode status = HttpStatusCode.OK)
            : base(data, message, metadata, status)
        {
            Success = true;
        }

        /// <summary>
        /// Creates a success result for paginated data
        /// </summary>
        public static ApiSuccessResult<T> WithPagination(T data, object paginationMetadata, string? message = null)
        {
            return new ApiSuccessResult<T>(
                data,
                message ?? ResponseMessages.RetrieveItemsSuccess,
                paginationMetadata,
                HttpStatusCode.OK
            );
        }
    }
}
