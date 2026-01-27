using MediatR;
using Microsoft.AspNetCore.Mvc;
using TLBIOMASS.Application.Features.Materials.Commands.CreateMaterial;
using TLBIOMASS.Application.Features.Materials.Commands.UpdateMaterial;
using TLBIOMASS.Application.Features.Materials.Commands.DeleteMaterial;
using TLBIOMASS.Application.Features.Materials.Queries.GetMaterials;
using TLBIOMASS.Application.Features.Materials.Queries.GetAllMaterials;
using TLBIOMASS.Application.Features.Materials.Queries.GetMaterialById;
using TLBIOMASS.Application.Features.Materials.DTOs;
using Mapster;
using Infrastructure.Common;
using Asp.Versioning;

namespace TLBIOMASS.API.Controllers;

[ApiVersion("1.0")]
public class MaterialController : ApiControllerBase<MaterialController>
{
    private const string EntityName = "Material";

    public MaterialController(IMediator mediator, ILogger<MaterialController> logger) 
        : base(mediator, logger)
    {
    }

    // GET /api/v1/material
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] string? sortDirection = "desc")
    {
        var query = new GetMaterialsQuery 
        { 
            Page = page, 
            Size = size, 
            Search = search, 
            IsActive = isActive, 
            SortBy = sortBy, 
            SortDirection = sortDirection 
        };

        return await HandleGetPagedAsync<GetMaterialsQuery, MaterialResponseDto>(
            query, EntityName, page, size);
    }

    // GET /api/v1/material/all
    [HttpGet("all")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetAllMaterialsQuery 
        { 
            Search = search, 
            IsActive = isActive 
        };

        return await HandleGetAllAsync<GetAllMaterialsQuery, MaterialResponseDto>(query, EntityName);
    }

    // GET /api/v1/material/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetMaterialByIdQuery { Id = id };
        return await HandleGetByIdAsync<GetMaterialByIdQuery, MaterialResponseDto>(query, id, EntityName);
    }

    // POST /api/v1/material
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MaterialCreateDto dto)
    {
        var command = dto.Adapt<CreateMaterialCommand>();
        return await HandleCreateAsync(command, EntityName, dto.Name);
    }

    // PUT /api/v1/material/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MaterialUpdateDto dto)
    {
        var command = dto.Adapt<UpdateMaterialCommand>();
        command.Id = id;
        return await HandleUpdateAsync(command, id, dto.Id, EntityName);
    }

    // DELETE /api/v1/material/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteMaterialCommand(id);
        return await HandleDeleteAsync(command, id, EntityName);
    }
}
