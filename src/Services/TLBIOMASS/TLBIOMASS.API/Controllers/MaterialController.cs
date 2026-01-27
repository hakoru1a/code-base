using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TLBIOMASS.Application.Features.Materials.Commands.CreateMaterial;
using TLBIOMASS.Application.Features.Materials.Commands.UpdateMaterial;
using TLBIOMASS.Application.Features.Materials.Commands.DeleteMaterial;
using TLBIOMASS.Application.Features.Materials.Queries.GetMaterials;
using TLBIOMASS.Application.Features.Materials.Queries.GetAllMaterials;
using TLBIOMASS.Application.Features.Materials.Queries.GetMaterialById;
using Shared.DTOs.Material;
using Mapster;
using Infrastructure.Common;
using Shared.SeedWork;

namespace TLBIOMASS.API.Controllers
{
    [ApiVersion("2.0")]
    public class MaterialController : ApiControllerBase<MaterialController>
    {
        private const string EntityName = "Material";

        public MaterialController(IMediator mediator, ILogger<MaterialController> logger)
            : base(mediator, logger)
        {
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiSuccessResult<List<MaterialResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetList([FromQuery] MaterialFilterDto filter)
        {
            var query = new GetAllMaterialsQuery { Filter = filter };
            return await HandleGetAllAsync<GetAllMaterialsQuery, MaterialResponseDto>(query, EntityName);
        }

        [HttpGet("paged")]
        [ProducesResponseType(typeof(ApiSuccessResult<List<MaterialResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPagedList([FromQuery] MaterialFilterDto filter)
        {
            var query = new GetMaterialsQuery { Filter = filter };
            return await HandleGetPagedAsync<GetMaterialsQuery, MaterialResponseDto>(
                query, EntityName, filter.PageNumber, filter.PageSize);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<MaterialResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<MaterialResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetMaterialByIdQuery { Id = id };
            return await HandleGetByIdAsync<GetMaterialByIdQuery, MaterialResponseDto>(query, id, EntityName);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiSuccessResult<int>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] MaterialCreateDto dto)
        {
            var command = dto.Adapt<CreateMaterialCommand>();
            return await HandleCreateAsync(command, EntityName, dto.Name);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] MaterialUpdateDto dto)
        {
            var command = dto.Adapt<UpdateMaterialCommand>();
            command.Id = id;
            return await HandleUpdateAsync(command, id, dto.Id, EntityName);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteMaterialCommand(id);
            return await HandleDeleteAsync(command, id, EntityName);
        }
    }
}
