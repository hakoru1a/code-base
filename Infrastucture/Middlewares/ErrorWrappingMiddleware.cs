using Contracts.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Infrastructure.Middlewares
{
    public class ErrorWrappingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorWrappingMiddleware> _logger;

        public ErrorWrappingMiddleware(RequestDelegate next, ILogger<ErrorWrappingMiddleware> logger)
        {
            _next = next;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse
            {
                Success = false,
                Message = exception.Message
            };

            switch (exception)
            {
                case DuplicateException duplicateEx:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse.ErrorCode = "DUPLICATE_ERROR";
                    break;
                case NotFoundException notFoundEx:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.ErrorCode = "NOT_FOUND";
                    break;
                case ValidationException validationEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.ErrorCode = "VALIDATION_ERROR";
                    errorResponse.ValidationErrors = validationEx.Errors;
                    break;
                case UnauthorizedException unauthorizedEx:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.ErrorCode = unauthorizedEx.ErrorCode ?? "UNAUTHORIZED";
                    break;
                case BadRequestException badRequestEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.ErrorCode = badRequestEx.ErrorCode ?? "BAD_REQUEST";
                    errorResponse.ValidationErrors = badRequestEx.ValidationErrors;
                    break;
                case BusinessException businessEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.ErrorCode = businessEx.ErrorCode ?? "BUSINESS_ERROR";
                    errorResponse.Details = businessEx.Details;
                    break;
                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.ErrorCode = "NOT_FOUND";
                    break;
                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.ErrorCode = "UNAUTHORIZED";
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Message = "An unexpected error occurred.";
                    errorResponse.ErrorCode = "INTERNAL_SERVER_ERROR";
                    break;
            }

            var result = JsonSerializer.Serialize(errorResponse);
            await response.WriteAsync(result);
        }
    }

    public class ErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public object? Details { get; set; }
        public IDictionary<string, string[]>? ValidationErrors { get; set; }
    }


}