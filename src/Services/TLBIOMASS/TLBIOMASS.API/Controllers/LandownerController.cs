using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TLBIOMASS.Application.Features.Landowners.Commands.CreateLandowner;
using TLBIOMASS.Application.Features.Landowners.Commands.UpdateLandowner;
using TLBIOMASS.Application.Features.Landowners.Commands.DeleteLandowner;
using TLBIOMASS.Application.Features.Landowners.Queries.GetLandowners;
using TLBIOMASS.Application.Features.Landowners.Queries.GetAllLandowners;
using TLBIOMASS.Application.Features.Landowners.Queries.GetLandownerById;
using Shared.DTOs.Landowner;
using Mapster;
using Infrastructure.Common;
using Shared.SeedWork;

namespace TLBIOMASS.API.Controllers
{
    [ApiVersion("2.0")]
    public class LandownerController : ApiControllerBase<LandownerController>
    {
        private const string EntityName = "Landowner";

        public LandownerController(IMediator mediator, ILogger<LandownerController> logger)
            : base(mediator, logger)
        {
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiSuccessResult<List<LandownerResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetList([FromQuery] LandownerFilterDto filter)
        {
            var query = new GetAllLandownersQuery
            {
                Search = filter.Search,
                IsActive = filter.IsActive
            };

            return await HandleGetAllAsync<GetAllLandownersQuery, LandownerResponseDto>(query, EntityName);
        }

        [HttpGet("paged")]
        [ProducesResponseType(typeof(ApiSuccessResult<List<LandownerResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPagedList([FromQuery] LandownerFilterDto filter)
        {
            var query = new GetLandownersQuery
            {
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Search = filter.Search,
                IsActive = filter.IsActive
            };

            return await HandleGetPagedAsync<GetLandownersQuery, LandownerResponseDto>(
                query, EntityName, filter.PageNumber, filter.PageSize);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<LandownerResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<LandownerResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetLandownerByIdQuery { Id = id };
            return await HandleGetByIdAsync<GetLandownerByIdQuery, LandownerResponseDto>(query, id, EntityName);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiSuccessResult<int>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] LandownerCreateDto dto)
        {
            var command = dto.Adapt<CreateLandownerCommand>();
            return await HandleCreateAsync(command, EntityName, dto.Name);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] LandownerUpdateDto dto)
        {
            var command = dto.Adapt<UpdateLandownerCommand>();
            command.Id = id;
            return await HandleUpdateAsync(command, id, dto.Id, EntityName);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteLandownerCommand(id);
            return await HandleDeleteAsync(command, id, EntityName);
        }
    }
}
