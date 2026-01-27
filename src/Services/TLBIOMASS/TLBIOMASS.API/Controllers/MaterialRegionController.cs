using MediatR;
using Microsoft.AspNetCore.Mvc;
using TLBIOMASS.Application.Features.MaterialRegions.Commands.CreateMaterialRegion;
using TLBIOMASS.Application.Features.MaterialRegions.Commands.UpdateMaterialRegion;
using TLBIOMASS.Application.Features.MaterialRegions.Commands.DeleteMaterialRegion;
using TLBIOMASS.Application.Features.MaterialRegions.Queries.GetAllMaterialRegions;
using TLBIOMASS.Application.Features.MaterialRegions.Queries.GetMaterialRegions;
using TLBIOMASS.Application.Features.MaterialRegions.Queries.GetMaterialRegionById;
using TLBIOMASS.Application.Features.MaterialRegions.DTOs;
using Mapster;
using Infrastructure.Common;
using Asp.Versioning;

namespace TLBIOMASS.API.Controllers;

[ApiVersion("1.0")]
public class MaterialRegionController : ApiControllerBase<MaterialRegionController>
{
    private const string EntityName = "MaterialRegion";

    public MaterialRegionController(IMediator mediator, ILogger<MaterialRegionController> logger) 
        : base(mediator, logger)
    {
    }

    // GET /api/v1/materialregion
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? ownerId = null,
        [FromQuery] string? sortBy = "CreatedDate",
        [FromQuery] string? sortDirection = "desc")
    {
        var query = new GetMaterialRegionsQuery 
        { 
            Page = page, 
            Size = size, 
            Search = search, 
            OwnerId = ownerId, 
            SortBy = sortBy, 
            SortDirection = sortDirection 
        };

        return await HandleGetPagedAsync<GetMaterialRegionsQuery, MaterialRegionResponseDto>(
            query, EntityName, page, size);
    }

    // GET /api/v1/materialregion/all
    [HttpGet("all")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] int? ownerId = null)
    {
        var query = new GetAllMaterialRegionsQuery 
        { 
            Search = search, 
            OwnerId = ownerId 
        };

        return await HandleGetAllAsync<GetAllMaterialRegionsQuery, MaterialRegionResponseDto>(query, EntityName);
    }

    // GET /api/v1/materialregion/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetMaterialRegionByIdQuery { Id = id };
        return await HandleGetByIdAsync<GetMaterialRegionByIdQuery, MaterialRegionResponseDto>(query, id, EntityName);
    }

    // POST /api/v1/materialregion
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MaterialRegionCreateDto dto)
    {
        var command = dto.Adapt<CreateMaterialRegionCommand>();
        return await HandleCreateAsync(command, EntityName, dto.RegionName);
    }

    // PUT /api/v1/materialregion/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MaterialRegionUpdateDto dto)
    {
        var command = dto.Adapt<UpdateMaterialRegionCommand>();
        command.Id = id;
        return await HandleUpdateAsync(command, id, dto.Id, EntityName);
    }

    // DELETE /api/v1/materialregion/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteMaterialRegionCommand(id);
        return await HandleDeleteAsync(command, id, EntityName);
    }
}
