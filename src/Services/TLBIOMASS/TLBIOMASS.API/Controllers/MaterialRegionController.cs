using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TLBIOMASS.Application.Features.MaterialRegions.Commands.CreateMaterialRegion;
using TLBIOMASS.Application.Features.MaterialRegions.Commands.UpdateMaterialRegion;
using TLBIOMASS.Application.Features.MaterialRegions.Commands.DeleteMaterialRegion;
using TLBIOMASS.Application.Features.MaterialRegions.Queries.GetMaterialRegions;
using TLBIOMASS.Application.Features.MaterialRegions.Queries.GetMaterialRegionsPaged;
using TLBIOMASS.Application.Features.MaterialRegions.Queries.GetMaterialRegionById;
using Shared.DTOs.MaterialRegion;
using Mapster;
using Infrastructure.Common;
using Shared.SeedWork;

namespace TLBIOMASS.API.Controllers
{
    [ApiVersion("1.0")]
    public class MaterialRegionController : ApiControllerBase<MaterialRegionController>
    {
        private const string EntityName = "MaterialRegion";

        public MaterialRegionController(IMediator mediator, ILogger<MaterialRegionController> logger)
            : base(mediator, logger)
        {
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiSuccessResult<List<MaterialRegionResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetList([FromQuery] MaterialRegionFilterDto filter)
        {
            var query = new GetMaterialRegionsQuery { Filter = filter };
            return await HandleGetAllAsync<GetMaterialRegionsQuery, MaterialRegionResponseDto>(query, EntityName);
        }

        [HttpGet("paged")]
        [ProducesResponseType(typeof(ApiSuccessResult<List<MaterialRegionResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPagedList([FromQuery] MaterialRegionPagedFilterDto filter)
        {
            var query = new GetMaterialRegionsPagedQuery { Filter = filter };
            return await HandleGetPagedAsync<GetMaterialRegionsPagedQuery, MaterialRegionResponseDto>(
                query, EntityName, filter.PageNumber, filter.PageSize);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<MaterialRegionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<MaterialRegionResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetMaterialRegionByIdQuery { Id = id };
            return await HandleGetByIdAsync<GetMaterialRegionByIdQuery, MaterialRegionResponseDto>(query, id, EntityName);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiSuccessResult<int>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] MaterialRegionCreateDto dto)
        {
            var command = dto.Adapt<CreateMaterialRegionCommand>();
            return await HandleCreateAsync(command, EntityName, dto.RegionName);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] MaterialRegionUpdateDto dto)
        {
            var command = dto.Adapt<UpdateMaterialRegionCommand>();
            command.Id = id;
            return await HandleUpdateAsync(command, id, dto.Id, EntityName);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteMaterialRegionCommand(id);
            return await HandleDeleteAsync(command, id, EntityName);
        }
    }
}
