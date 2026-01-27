using MediatR;
using Microsoft.AspNetCore.Mvc;
using TLBIOMASS.Application.Features.Agencies.Commands.CreateAgency;
using TLBIOMASS.Application.Features.Agencies.Commands.UpdateAgency;
using TLBIOMASS.Application.Features.Agencies.Commands.DeleteAgency;
using TLBIOMASS.Application.Features.Agencies.Queries.GetAgencies;
using TLBIOMASS.Application.Features.Agencies.Queries.GetAllAgencies;
using TLBIOMASS.Application.Features.Agencies.Queries.GetAgencyById;
using TLBIOMASS.Application.Features.Agencies.DTOs;
using Mapster;
using Infrastructure.Common;
using Asp.Versioning;

namespace TLBIOMASS.API.Controllers;

[ApiVersion("1.0")]
public class AgencyController : ApiControllerBase<AgencyController>
{
    private const string EntityName = "Agency";

    public AgencyController(IMediator mediator, ILogger<AgencyController> logger) 
        : base(mediator, logger)
    {
    }

    // GET /api/v1/agency
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = "CreatedDate",
        [FromQuery] string? sortDirection = "desc")
    {
        var query = new GetAgenciesQuery 
        { 
            Page = page, 
            Size = size, 
            Search = search, 
            IsActive = isActive, 
            SortBy = sortBy, 
            SortDirection = sortDirection 
        };

        return await HandleGetPagedAsync<GetAgenciesQuery, AgencyResponseDto>(
            query, EntityName, page, size);
    }

    // GET /api/v1/agency/all
    [HttpGet("all")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetAllAgenciesQuery 
        { 
            Search = search, 
            IsActive = isActive 
        };

        return await HandleGetAllAsync<GetAllAgenciesQuery, AgencyResponseDto>(query, EntityName);
    }

    // GET /api/v1/agency/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetAgencyByIdQuery { Id = id };
        return await HandleGetByIdAsync<GetAgencyByIdQuery, AgencyResponseDto>(query, id, EntityName);
    }

    // POST /api/v1/agency
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AgencyCreateDto dto)
    {
        var command = dto.Adapt<CreateAgencyCommand>();
        return await HandleCreateAsync(command, EntityName, dto.Name);
    }

    // PUT /api/v1/agency/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AgencyUpdateDto dto)
    {
        var command = dto.Adapt<UpdateAgencyCommand>();
        command.Id = id;
        return await HandleUpdateAsync(command, id, dto.Id, EntityName);
    }

    // DELETE /api/v1/agency/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteAgencyCommand(id);
        return await HandleDeleteAsync(command, id, EntityName);
    }
}
