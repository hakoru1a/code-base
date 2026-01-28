using Contracts.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.SeedWork;
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

            object errorResponse;
            string message = exception.Message;

            switch (exception)
            {
                case DuplicateException duplicateEx:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse = new ApiErrorResult<object>(message, HttpStatusCode.Conflict);
                    break;

                case NotFoundException notFoundEx:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse = new ApiErrorResult<object>(message, HttpStatusCode.NotFound);
                    break;

                case ValidationException validationEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var validationErrors = validationEx.Errors?.SelectMany(kvp =>
                        kvp.Value.Select(error => $"{kvp.Key}: {error}"))
                        .ToList() ?? new List<string> { message };
                    errorResponse = new ApiErrorResult<object>(
                        ResponseMessages.ValidationFailed,
                        validationErrors,
                        HttpStatusCode.BadRequest);
                    break;

                case UnauthorizedException unauthorizedEx:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse = new ApiErrorResult<object>(message, HttpStatusCode.Unauthorized);
                    break;

                case BadRequestException badRequestEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    if (badRequestEx.ValidationErrors != null && badRequestEx.ValidationErrors.Any())
                    {
                        var errors = badRequestEx.ValidationErrors
                            .SelectMany(kvp => kvp.Value.Select(error => $"{kvp.Key}: {error}"))
                            .ToList();
                        errorResponse = new ApiErrorResult<object>(message, errors, HttpStatusCode.BadRequest);
                    }
                    else
                    {
                        errorResponse = new ApiErrorResult<object>(message, HttpStatusCode.BadRequest);
                    }
                    break;

                case BusinessException businessEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = new ApiErrorResult<object>(message, HttpStatusCode.BadRequest);
                    break;

                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse = new ApiErrorResult<object>("Resource not found", HttpStatusCode.NotFound);
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse = new ApiErrorResult<object>("Unauthorized access", HttpStatusCode.Unauthorized);
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse = new ApiErrorResult<object>(HttpStatusCode.InternalServerError);
                    break;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var result = JsonSerializer.Serialize(errorResponse, options);
            await response.WriteAsync(result);
        }
    }
}
