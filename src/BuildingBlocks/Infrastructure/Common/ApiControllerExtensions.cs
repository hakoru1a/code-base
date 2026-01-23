using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.SeedWork;
using System.Net;

namespace Infrastructure.Common;

/// <summary>
/// Extension methods for ApiControllerBase to support additional common operations
/// </summary>
public static class ApiControllerExtensions
{
    /// <summary>
    /// Create a standardized success response
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="controller">Controller instance</param>
    /// <param name="data">Response data</param>
    /// <param name="message">Success message</param>
    /// <returns>Ok result with standardized response</returns>
    public static IActionResult CreateSuccessResponse<T>(this ControllerBase controller, T data, string? message = null)
    {
        var responseMessage = message ?? ResponseMessages.OperationSuccess;
        return controller.Ok(new ApiSuccessResult<T>(data, responseMessage));
    }

    /// <summary>
    /// Create a standardized success response with metadata
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="controller">Controller instance</param>
    /// <param name="data">Response data</param>
    /// <param name="metadata">Metadata object</param>
    /// <param name="message">Success message</param>
    /// <returns>Ok result with standardized response and metadata</returns>
    public static IActionResult CreateSuccessResponse<T>(this ControllerBase controller, T data, object metadata, string? message = null)
    {
        var responseMessage = message ?? ResponseMessages.OperationSuccess;
        return controller.Ok(new ApiSuccessResult<T>(data, responseMessage, metadata));
    }

    /// <summary>
    /// Create a standardized error response
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="controller">Controller instance</param>
    /// <param name="message">Error message</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <returns>Error result with standardized response</returns>
    public static IActionResult CreateErrorResponse<T>(this ControllerBase controller, string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        var result = new ApiErrorResult<T>(message, statusCode);
        return statusCode switch
        {
            HttpStatusCode.BadRequest => controller.BadRequest(result),
            HttpStatusCode.Unauthorized => controller.Unauthorized(result),
            HttpStatusCode.Forbidden => controller.Forbid(),
            HttpStatusCode.NotFound => controller.NotFound(result),
            HttpStatusCode.Conflict => controller.Conflict(result),
            HttpStatusCode.UnprocessableEntity => controller.UnprocessableEntity(result),
            HttpStatusCode.InternalServerError => controller.StatusCode(500, result),
            _ => controller.BadRequest(result)
        };
    }

    /// <summary>
    /// Log and create not found response for entity
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="controller">Controller instance</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="entityName">Entity name</param>
    /// <param name="id">Entity ID</param>
    /// <returns>NotFound result</returns>
    public static IActionResult CreateNotFoundResponse<T>(this ControllerBase controller, ILogger logger, string entityName, long id)
    {
        logger.LogWarning("{EntityName} with ID: {Id} not found", entityName, id);
        return controller.NotFound(new ApiErrorResult<T>(
            ResponseMessages.ItemNotFound(entityName, id), HttpStatusCode.NotFound));
    }

    /// <summary>
    /// Log and create validation error response
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="controller">Controller instance</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="message">Validation error message</param>
    /// <returns>BadRequest result</returns>
    public static IActionResult CreateValidationErrorResponse<T>(this ControllerBase controller, ILogger logger, string message)
    {
        logger.LogWarning("Validation error: {Message}", message);
        return controller.BadRequest(new ApiErrorResult<T>(message, HttpStatusCode.BadRequest));
    }
}