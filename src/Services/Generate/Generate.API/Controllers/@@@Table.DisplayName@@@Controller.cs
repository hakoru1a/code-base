@@@copyRight@@@
using Asp.Versioning;
using Shared.DTOs.@@@Table.DisplayName@@@;
using Generate.Application.Features.@@@Table.DisplayName@@@.Commands.Create @@@Table.DisplayName@@@;
using Generate.Application.Features.@@@Table.DisplayName@@@.Commands.Delete @@@Table.DisplayName@@@;
using Generate.Application.Features.@@@Table.DisplayName@@@.Commands.Update @@@Table.DisplayName@@@;
using Generate.Application.Features.@@@Table.DisplayName@@@.Queries.Get @@@Table.DisplayName @@@ById;
using Generate.Application.Features.@@@Table.DisplayName@@@.Queries.Get @@@Table.DisplayName @@@s;
using Infrastructure.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Attributes;
using Shared.SeedWork;

namespace Generate.API.Controllers
{
    /// <summary>
    /// Controller for managing @@@Table.DisplayName@@@s
    /// </summary>
    [ApiVersion("1.0")]
    public class @@@Table.DisplayName @@@Controller : ApiControllerBase<@@@Table.DisplayName@@@Controller>
    {
        private const string EntityName = "@@@Table.DisplayName@@@";

    public @@@Table.DisplayName @@@Controller(IMediator mediator, ILogger<@@@Table.DisplayName@@@Controller> logger)
            : base(mediator, logger)
        {
    }

    /// <summary>
    /// Get all @@@Table.DisplayName@@@s - requires @@@Table.DisplayName@@@:VIEW policy
    /// </summary>
    /// <returns>List of @@@Table.DisplayName@@@s</returns>
    /// <response code="200">Returns the list of @@@Table.DisplayName@@@s</response>
    /// <response code="403">Forbidden - User lacks required permissions</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [RequirePolicy("@@@Table.DisplayName@@@:VIEW")]
    [ProducesResponseType(typeof(ApiSuccessResult<List<@@@Table.DisplayName@@@ResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetList()
    {
        var query = new Get@@@Table.DisplayName @@@sQuery();
        return await HandleGetAllAsync < Get@@@Table.DisplayName @@@sQuery, @@@Table.DisplayName @@@ResponseDto> (query, EntityName);
    }

    /// <summary>
    /// Get @@@Table.DisplayName@@@ by ID - requires @@@Table.DisplayName@@@:VIEW policy
    /// </summary>
    /// <param name="id">@@@Table.DisplayName@@@ ID</param>
    /// <returns>@@@Table.DisplayName@@@ details</returns>
    /// <response code="200">Returns the @@@Table.DisplayName@@@</response>
    /// <response code="403">Forbidden - User lacks required permissions</response>
    /// <response code="404">@@@Table.DisplayName@@@ not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [RequirePolicy("@@@Table.DisplayName@@@:VIEW")]
    [ProducesResponseType(typeof(ApiSuccessResult<@@@Table.DisplayName@@@ResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResult<@@@Table.DisplayName@@@ResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(long id)
    {
        var query = new Get@@@Table.DisplayName @@@ByIdQuery { Id = id }
        ;
        return await HandleGetByIdAsync < Get@@@Table.DisplayName @@@ByIdQuery, @@@Table.DisplayName @@@ResponseDto> (query, id, EntityName);
    }

    /// <summary>
    /// Create a new @@@Table.DisplayName@@@ - requires @@@Table.DisplayName@@@:CREATE policy
    /// </summary>
    /// <param name="dto">@@@Table.DisplayName@@@ creation data</param>
    /// <returns>ID of the created @@@Table.DisplayName@@@</returns>
    /// <response code="201">@@@Table.DisplayName@@@ created successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="403">Forbidden - User lacks required permissions</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [RequirePolicy("@@@Table.DisplayName@@@:CREATE")]
    [ProducesResponseType(typeof(ApiSuccessResult<long>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] @@@Table.DisplayName @@@CreateDto dto)
    {
        var command = new Create@@@Table.DisplayName @@@Command
            {
< FOREACH >
    < LOOP > TableGenerate.ColumnsNotKey </ LOOP >
    < CONTENT >                ###ColumnName### = dto.###ColumnName###</CONTENT>
    < REMOVELAST >,</ REMOVELAST >
</ FOREACH >
            }
        ;

        return await HandleCreateAsync(command, EntityName, dto.ToString());
    }

    /// <summary>
    /// Update an existing @@@Table.DisplayName@@@ - requires @@@Table.DisplayName@@@:UPDATE policy
    /// </summary>
    /// <param name="id">@@@Table.DisplayName@@@ ID</param>
    /// <param name="dto">@@@Table.DisplayName@@@ update data</param>
    /// <returns>Update result</returns>
    /// <response code="200">@@@Table.DisplayName@@@ updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="403">Forbidden - User lacks required permissions</response>
    /// <response code="404">@@@Table.DisplayName@@@ not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [RequirePolicy("@@@Table.DisplayName@@@:UPDATE")]
    [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(long id, [FromBody] @@@Table.DisplayName @@@UpdateDto dto)
    {
        var command = new Update@@@Table.DisplayName @@@Command
            {
< FOREACH >
    < LOOP > TableGenerate.Columns </ LOOP >
    < CONTENT >                ###ColumnName### = dto.###ColumnName###</CONTENT>
    < REMOVELAST >,</ REMOVELAST >
</ FOREACH >
            }
        ;

        return await HandleUpdateAsync(command, id, dto.Id, EntityName);
    }

    /// <summary>
    /// Delete a @@@Table.DisplayName@@@ - requires @@@Table.DisplayName@@@:DELETE policy
    /// </summary>
    /// <param name="id">@@@Table.DisplayName@@@ ID</param>
    /// <returns>Delete result</returns>
    /// <response code="200">@@@Table.DisplayName@@@ deleted successfully</response>
    /// <response code="403">Forbidden - User lacks required permissions</response>
    /// <response code="404">@@@Table.DisplayName@@@ not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [RequirePolicy("@@@Table.DisplayName@@@:DELETE")]
    [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(long id)
    {
        var command = new Delete@@@Table.DisplayName @@@Command { Id = id }
        ;
        return await HandleDeleteAsync(command, id, EntityName);
    }
}
}