using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.SeedWork;

namespace Infrastructure.Common;

/// <summary>
/// Base class for API controllers providing common functionality
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase<T> : ControllerBase where T : class
{
    protected readonly IMediator Mediator;
    protected readonly ILogger<T> Logger;

    protected ApiControllerBase(IMediator mediator, ILogger<T> logger)
    {
        Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handle Get All operation
    /// </summary>
    /// <typeparam name="TQuery">Query type</typeparam>
    /// <typeparam name="TResponse">Response DTO type</typeparam>
    /// <param name="query">Query object</param>
    /// <param name="entityName">Entity name for logging</param>
    /// <returns>List of entities</returns>
    protected async Task<IActionResult> HandleGetAllAsync<TQuery, TResponse>(
        TQuery query, 
        string entityName)
        where TQuery : IRequest<List<TResponse>>
    {
        Logger.LogInformation("Getting all {EntityName}", entityName);
        var result = await Mediator.Send(query);
        return Ok(new ApiSuccessResult<List<TResponse>>(result, ResponseMessages.RetrieveItemsSuccess));
    }

    /// <summary>
    /// Handle Get Paged operation
    /// </summary>
    /// <typeparam name="TQuery">Query type</typeparam>
    /// <typeparam name="TResponse">Response DTO type</typeparam>
    /// <param name="query">Query object</param>
    /// <param name="entityName">Entity name for logging</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paged list of entities</returns>
    protected async Task<IActionResult> HandleGetPagedAsync<TQuery, TResponse>(
        TQuery query,
        string entityName,
        int pageNumber,
        int pageSize)
        where TQuery : IRequest<PagedList<TResponse>>
    {
        Logger.LogInformation("Getting paginated {EntityName} - Page: {PageNumber}, Size: {PageSize}",
            entityName, pageNumber, pageSize);

        var pagedResult = await Mediator.Send(query);
        var metadata = pagedResult.GetMetaData();

        Logger.LogInformation("Retrieved {Count} {EntityName} out of {TotalCount}",
            pagedResult.Count, entityName, metadata.TotalItems);

        return Ok(new ApiSuccessResult<List<TResponse>>(
            pagedResult.ToList(),
            ResponseMessages.RetrieveItemsSuccess,
            metadata));
    }

    /// <summary>
    /// Handle Get By ID operation
    /// </summary>
    /// <typeparam name="TQuery">Query type</typeparam>
    /// <typeparam name="TResponse">Response DTO type</typeparam>
    /// <param name="query">Query object</param>
    /// <param name="id">Entity ID</param>
    /// <param name="entityName">Entity name for logging</param>
    /// <returns>Entity or NotFound</returns>
    protected async Task<IActionResult> HandleGetByIdAsync<TQuery, TResponse>(
        TQuery query,
        long id,
        string entityName)
        where TQuery : IRequest<TResponse>
    {
        Logger.LogInformation("Getting {EntityName} with ID: {Id}", entityName, id);
        var result = await Mediator.Send(query);

        if (result == null)
        {
            Logger.LogWarning("{EntityName} with ID: {Id} not found", entityName, id);
            return NotFound(new ApiErrorResult<TResponse>(
                ResponseMessages.ItemNotFound(entityName, id)));
        }

        return Ok(new ApiSuccessResult<TResponse>(result, ResponseMessages.RetrieveItemSuccess));
    }

    /// <summary>
    /// Handle Create operation
    /// </summary>
    /// <typeparam name="TCommand">Command type</typeparam>
    /// <param name="command">Command object</param>
    /// <param name="entityName">Entity name for logging</param>
    /// <param name="entityIdentifier">Entity identifier for logging</param>
    /// <param name="getByIdActionName">Action name for CreatedAtAction</param>
    /// <returns>Created entity ID</returns>
    protected async Task<IActionResult> HandleCreateAsync<TCommand>(
        TCommand command,
        string entityName,
        string entityIdentifier,
        string getByIdActionName = "GetById")
        where TCommand : IRequest<long>
    {
        Logger.LogInformation("Creating new {EntityName}: {EntityIdentifier}", entityName, entityIdentifier);

        var entityId = await Mediator.Send(command);
        Logger.LogInformation("{EntityName} created successfully with ID: {EntityId}", entityName, entityId);

        return CreatedAtAction(
            getByIdActionName,
            new { id = entityId },
            new ApiSuccessResult<long>(entityId, ResponseMessages.ItemCreated(entityName))
        );
    }

    /// <summary>
    /// Handle Update operation
    /// </summary>
    /// <typeparam name="TCommand">Command type</typeparam>
    /// <param name="command">Command object</param>
    /// <param name="id">Entity ID from URL</param>
    /// <param name="dtoId">Entity ID from DTO</param>
    /// <param name="entityName">Entity name for logging</param>
    /// <returns>Update result</returns>
    protected async Task<IActionResult> HandleUpdateAsync<TCommand>(
        TCommand command,
        long id,
        long dtoId,
        string entityName)
        where TCommand : IRequest<bool>
    {
        Logger.LogInformation("Updating {EntityName} with ID: {Id}", entityName, id);

        if (id != dtoId)
        {
            Logger.LogWarning("ID mismatch - URL ID: {UrlId}, Body ID: {BodyId}", id, dtoId);
            return BadRequest(new ApiErrorResult<bool>("ID in URL does not match ID in body"));
        }

        var result = await Mediator.Send(command);

        if (!result)
        {
            Logger.LogWarning("{EntityName} with ID: {Id} not found for update", entityName, id);
            return NotFound(new ApiErrorResult<bool>(
                ResponseMessages.ItemNotFound(entityName, id)));
        }

        Logger.LogInformation("{EntityName} with ID: {Id} updated successfully", entityName, id);
        return Ok(new ApiSuccessResult<bool>(result, ResponseMessages.ItemUpdated(entityName)));
    }

    /// <summary>
    /// Handle Delete operation
    /// </summary>
    /// <typeparam name="TCommand">Command type</typeparam>
    /// <param name="command">Command object</param>
    /// <param name="id">Entity ID</param>
    /// <param name="entityName">Entity name for logging</param>
    /// <returns>Delete result</returns>
    protected async Task<IActionResult> HandleDeleteAsync<TCommand>(
        TCommand command,
        long id,
        string entityName)
        where TCommand : IRequest<bool>
    {
        Logger.LogInformation("Deleting {EntityName} with ID: {Id}", entityName, id);

        var result = await Mediator.Send(command);

        if (!result)
        {
            Logger.LogWarning("{EntityName} with ID: {Id} not found for deletion", entityName, id);
            return NotFound(new ApiErrorResult<bool>(
                ResponseMessages.ItemNotFound(entityName, id)));
        }

        Logger.LogInformation("{EntityName} with ID: {Id} deleted successfully", entityName, id);
        return Ok(new ApiSuccessResult<bool>(result, ResponseMessages.ItemDeleted(entityName)));
    }

    /// <summary>
    /// Validate ID mismatch between URL and body
    /// </summary>
    /// <param name="urlId">ID from URL</param>
    /// <param name="bodyId">ID from request body</param>
    /// <param name="entityName">Entity name for logging</param>
    /// <returns>True if IDs match, false otherwise</returns>
    protected bool ValidateIdMatch(long urlId, long bodyId, string entityName)
    {
        if (urlId != bodyId)
        {
            Logger.LogWarning("{EntityName} ID mismatch - URL ID: {UrlId}, Body ID: {BodyId}", 
                entityName, urlId, bodyId);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Create BadRequest response for ID mismatch
    /// </summary>
    /// <returns>BadRequest result</returns>
    protected IActionResult CreateIdMismatchResponse()
    {
        return BadRequest(new ApiErrorResult<bool>("ID in URL does not match ID in body"));
    }

    /// <summary>
    /// Create NotFound response for entity
    /// </summary>
    /// <typeparam name="TResult">Result type</typeparam>
    /// <param name="entityName">Entity name</param>
    /// <param name="id">Entity ID</param>
    /// <returns>NotFound result</returns>
    protected IActionResult CreateNotFoundResponse<TResult>(string entityName, long id)
    {
        return NotFound(new ApiErrorResult<TResult>(
            ResponseMessages.ItemNotFound(entityName, id)));
    }
}