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
            : base(data, ResponseMessages.RetrieveSuccess)
        {
        }

        public ApiSuccessResult(T data, string message)
            : base(data, message)
        {
        }

        public ApiSuccessResult(T data, string message, object metadata)
            : base(data, message, metadata)
        {
        }

        /// <summary>
        /// Creates a success result for paginated data
        /// </summary>
        public static ApiSuccessResult<T> WithPagination(T data, object paginationMetadata, string? message = null)
        {
            return new ApiSuccessResult<T>(
                data,
                message ?? ResponseMessages.RetrieveItemsSuccess,
                paginationMetadata
            );
        }
    }
}
