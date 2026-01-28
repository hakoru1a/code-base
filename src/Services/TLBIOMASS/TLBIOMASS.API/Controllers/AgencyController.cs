using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TLBIOMASS.Application.Features.Agencies.Commands.CreateAgency;
using TLBIOMASS.Application.Features.Agencies.Commands.UpdateAgency;
using TLBIOMASS.Application.Features.Agencies.Commands.DeleteAgency;
using TLBIOMASS.Application.Features.Agencies.Queries.GetAgencies;
using TLBIOMASS.Application.Features.Agencies.Queries.GetAllAgencies;
using TLBIOMASS.Application.Features.Agencies.Queries.GetAgencyById;
using Shared.DTOs.Agency;
using Mapster;
using Infrastructure.Common;
using Shared.SeedWork;

namespace TLBIOMASS.API.Controllers
{
    [ApiVersion("1.0")]
    public class AgencyController : ApiControllerBase<AgencyController>
    {
        private const string EntityName = "Agency";

        public AgencyController(IMediator mediator, ILogger<AgencyController> logger)
            : base(mediator, logger)
        {
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiSuccessResult<List<AgencyResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetList([FromQuery] AgencyFilterDto filter)
        {
            var query = new GetAllAgenciesQuery { Filter = filter };
            return await HandleGetAllAsync<GetAllAgenciesQuery, AgencyResponseDto>(query, EntityName);
        }

        [HttpGet("paged")]
        [ProducesResponseType(typeof(ApiSuccessResult<List<AgencyResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPagedList([FromQuery] AgencyPagedFilterDto filter)
        {
            var query = new GetAgenciesQuery { Filter = filter };
            return await HandleGetPagedAsync<GetAgenciesQuery, AgencyResponseDto>(
                query, EntityName, filter.PageNumber, filter.PageSize);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<AgencyResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<AgencyResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetAgencyByIdQuery { Id = id };
            return await HandleGetByIdAsync<GetAgencyByIdQuery, AgencyResponseDto>(query, id, EntityName);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiSuccessResult<int>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AgencyCreateDto dto)
        {
            var command = dto.Adapt<CreateAgencyCommand>();
            return await HandleCreateAsync(command, EntityName, dto.Name);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] AgencyUpdateDto dto)
        {
            var command = dto.Adapt<UpdateAgencyCommand>();
            command.Id = id;
            return await HandleUpdateAsync(command, id, dto.Id, EntityName);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteAgencyCommand(id);
            return await HandleDeleteAsync(command, id, EntityName);
        }
    }
}
