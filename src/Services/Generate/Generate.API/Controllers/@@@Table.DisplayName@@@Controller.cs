@@@copyRight@@@
using Asp.Versioning;
using Shared.DTOs.@@@Table.Name@@@;
using Generate.Application.Features.@@@Table.Name@@@.Commands.Create@@@Table.Name@@@;
using Generate.Application.Features.@@@Table.Name@@@.Commands.Delete@@@Table.Name@@@;
using Generate.Application.Features.@@@Table.Name@@@.Commands.Update@@@Table.Name@@@;
using Generate.Application.Features.@@@Table.Name@@@.Queries.Get@@@Table.Name@@@ById;
using Generate.Application.Features.@@@Table.Name@@@.Queries.Get@@@Table.Name@@@s;
using Infrastructure.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Attributes;
using Shared.SeedWork;

namespace Generate.API.Controllers
{
    /// <summary>
    /// Controller for managing @@@Table.Name@@@s
    /// </summary>
    [ApiVersion("1.0")]
    public class @@@Table.Name@@@Controller : ApiControllerBase<@@@Table.Name@@@Controller>
    {
        private const string EntityName = "@@@Table.Name@@@";

        public @@@Table.Name@@@Controller(IMediator mediator, ILogger<@@@Table.Name@@@Controller> logger)
            : base(mediator, logger)
        {
        }

        /// <summary>
        /// Get all @@@Table.Name@@@s - requires @@@Table.Name@@@:VIEW policy
        /// </summary>
        /// <returns>List of @@@Table.Name@@@s</returns>
        /// <response code="200">Returns the list of @@@Table.Name@@@s</response>
        /// <response code="403">Forbidden - User lacks required permissions</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [RequirePolicy("@@@Table.Name@@@:VIEW")]
        [ProducesResponseType(typeof(ApiSuccessResult<List<@@@Table.Name@@@ResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetList()
        {
            var query = new Get@@@Table.Name@@@sQuery();
            return await HandleGetAllAsync<Get@@@Table.Name@@@sQuery, @@@Table.Name@@@ResponseDto>(query, EntityName);
        }

        /// <summary>
        /// Get @@@Table.Name@@@ by ID - requires @@@Table.Name@@@:VIEW policy
        /// </summary>
        /// <param name="id">@@@Table.Name@@@ ID</param>
        /// <returns>@@@Table.Name@@@ details</returns>
        /// <response code="200">Returns the @@@Table.Name@@@</response>
        /// <response code="403">Forbidden - User lacks required permissions</response>
        /// <response code="404">@@@Table.Name@@@ not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [RequirePolicy("@@@Table.Name@@@:VIEW")]
        [ProducesResponseType(typeof(ApiSuccessResult<@@@Table.Name@@@ResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResult<@@@Table.Name@@@ResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(long id)
        {
            var query = new Get@@@Table.Name@@@ByIdQuery { Id = id };
            return await HandleGetByIdAsync<Get@@@Table.Name@@@ByIdQuery, @@@Table.Name@@@ResponseDto>(query, id, EntityName);
        }

        /// <summary>
        /// Create a new @@@Table.Name@@@ - requires @@@Table.Name@@@:CREATE policy
        /// </summary>
        /// <param name="dto">@@@Table.Name@@@ creation data</param>
        /// <returns>ID of the created @@@Table.Name@@@</returns>
        /// <response code="201">@@@Table.Name@@@ created successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="403">Forbidden - User lacks required permissions</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [RequirePolicy("@@@Table.Name@@@:CREATE")]
        [ProducesResponseType(typeof(ApiSuccessResult<long>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] @@@Table.Name@@@CreateDto dto)
        {
            var command = new Create@@@Table.Name@@@Command
            {
<FOREACH>
    <LOOP>TableGenerate.ColumnsNotKey</LOOP>
    <CONTENT>                ###ColumnName### = dto.###ColumnName###</CONTENT>
    <REMOVELAST>,</REMOVELAST>
</FOREACH>
            };

            return await HandleCreateAsync(command, EntityName, dto.ToString());
        }

        /// <summary>
        /// Update an existing @@@Table.Name@@@ - requires @@@Table.Name@@@:UPDATE policy
        /// </summary>
        /// <param name="id">@@@Table.Name@@@ ID</param>
        /// <param name="dto">@@@Table.Name@@@ update data</param>
        /// <returns>Update result</returns>
        /// <response code="200">@@@Table.Name@@@ updated successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="403">Forbidden - User lacks required permissions</response>
        /// <response code="404">@@@Table.Name@@@ not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        [RequirePolicy("@@@Table.Name@@@:UPDATE")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(long id, [FromBody] @@@Table.Name@@@UpdateDto dto)
        {
            var command = new Update@@@Table.Name@@@Command
            {
<FOREACH>
    <LOOP>TableGenerate.Columns</LOOP>
    <CONTENT>                ###ColumnName### = dto.###ColumnName###</CONTENT>
    <REMOVELAST>,</REMOVELAST>
</FOREACH>
            };

            return await HandleUpdateAsync(command, id, dto.Id, EntityName);
        }

        /// <summary>
        /// Delete a @@@Table.Name@@@ - requires @@@Table.Name@@@:DELETE policy
        /// </summary>
        /// <param name="id">@@@Table.Name@@@ ID</param>
        /// <returns>Delete result</returns>
        /// <response code="200">@@@Table.Name@@@ deleted successfully</response>
        /// <response code="403">Forbidden - User lacks required permissions</response>
        /// <response code="404">@@@Table.Name@@@ not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [RequirePolicy("@@@Table.Name@@@:DELETE")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id)
        {
            var command = new Delete@@@Table.Name@@@Command { Id = id };
            return await HandleDeleteAsync(command, id, EntityName);
        }
    }
}