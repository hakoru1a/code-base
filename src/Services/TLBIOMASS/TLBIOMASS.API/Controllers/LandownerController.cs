using MediatR;
using Microsoft.AspNetCore.Mvc;
using TLBIOMASS.Application.Features.Landowners.Commands.CreateLandowner;
using TLBIOMASS.Application.Features.Landowners.Commands.UpdateLandowner;
using TLBIOMASS.Application.Features.Landowners.Commands.DeleteLandowner;
using TLBIOMASS.Application.Features.Landowners.Queries.GetLandowners;
using TLBIOMASS.Application.Features.Landowners.Queries.GetAllLandowners;
using TLBIOMASS.Application.Features.Landowners.Queries.GetLandownerById;
using TLBIOMASS.Application.Features.Landowners.DTOs;
using Mapster;
using Infrastructure.Common;
using Asp.Versioning;

namespace TLBIOMASS.API.Controllers;

[ApiVersion("1.0")]
public class LandownerController : ApiControllerBase<LandownerController>
{
    private const string EntityName = "Landowner";

    public LandownerController(IMediator mediator, ILogger<LandownerController> logger) 
        : base(mediator, logger)
    {
    }

    // GET /api/v1/landowner
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = "CreatedDate",
        [FromQuery] string? sortDirection = "desc")
    {
        var query = new GetLandownersQuery 
        { 
            Page = page, 
            Size = size, 
            Search = search, 
            IsActive = isActive, 
            SortBy = sortBy, 
            SortDirection = sortDirection 
        };

        return await HandleGetPagedAsync<GetLandownersQuery, LandownerResponseDto>(
            query, EntityName, page, size);
    }

    // GET /api/v1/landowner/all
    [HttpGet("all")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetAllLandownersQuery 
        { 
            Search = search, 
            IsActive = isActive 
        };

        return await HandleGetAllAsync<GetAllLandownersQuery, LandownerResponseDto>(query, EntityName);
    }

    // GET /api/v1/landowner/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetLandownerByIdQuery { Id = id };
        return await HandleGetByIdAsync<GetLandownerByIdQuery, LandownerResponseDto>(query, id, EntityName);
    }

    // POST /api/v1/landowner
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LandownerCreateDto dto)
    {
        var command = dto.Adapt<CreateLandownerCommand>();
        return await HandleCreateAsync(command, EntityName, dto.Name);
    }

    // PUT /api/v1/landowner/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] LandownerUpdateDto dto)
    {
        var command = dto.Adapt<UpdateLandownerCommand>();
        command.Id = id;
        return await HandleUpdateAsync(command, id, dto.Id, EntityName);
    }

    // DELETE /api/v1/landowner/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteLandownerCommand(id);
        return await HandleDeleteAsync(command, id, EntityName);
    }
}
