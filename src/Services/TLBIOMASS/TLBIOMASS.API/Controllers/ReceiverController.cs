using MediatR;
using Microsoft.AspNetCore.Mvc;
using TLBIOMASS.Application.Features.Receivers.Commands.CreateReceiver;
using TLBIOMASS.Application.Features.Receivers.Commands.UpdateReceiver;
using TLBIOMASS.Application.Features.Receivers.Commands.DeleteReceiver;
using TLBIOMASS.Application.Features.Receivers.Queries.GetReceivers;
using TLBIOMASS.Application.Features.Receivers.Queries.GetAllReceivers;
using TLBIOMASS.Application.Features.Receivers.Queries.GetReceiverById;
using TLBIOMASS.Application.Features.Receivers.DTOs;
using Mapster;
using Infrastructure.Common;
using Asp.Versioning;

namespace TLBIOMASS.API.Controllers;

[ApiVersion("1.0")]
public class ReceiverController : ApiControllerBase<ReceiverController>
{
    private const string EntityName = "Receiver";

    public ReceiverController(IMediator mediator, ILogger<ReceiverController> logger) 
        : base(mediator, logger)
    {
    }

    // GET /api/v1/receiver
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] string? sortDirection = "desc")
    {
        var query = new GetReceiversQuery 
        { 
            Page = page, 
            Size = size, 
            Search = search, 
            IsActive = isActive, 
            SortBy = sortBy, 
            SortDirection = sortDirection 
        };

        return await HandleGetPagedAsync<GetReceiversQuery, ReceiverResponseDto>(
            query, EntityName, page, size);
    }

    // GET /api/v1/receiver/all
    [HttpGet("all")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetAllReceiversQuery 
        { 
            Search = search, 
            IsActive = isActive 
        };

        return await HandleGetAllAsync<GetAllReceiversQuery, ReceiverResponseDto>(query, EntityName);
    }

    // GET /api/v1/receiver/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetReceiverByIdQuery { Id = id };
        return await HandleGetByIdAsync<GetReceiverByIdQuery, ReceiverResponseDto>(query, id, EntityName);
    }

    // POST /api/v1/receiver
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReceiverCreateDto dto)
    {
        var command = dto.Adapt<CreateReceiverCommand>();
        return await HandleCreateAsync(command, EntityName, dto.Name);
    }

    // PUT /api/v1/receiver/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ReceiverUpdateDto dto)
    {
        var command = dto.Adapt<UpdateReceiverCommand>();
        command.Id = id;
        return await HandleUpdateAsync(command, id, dto.Id, EntityName);
    }

    // DELETE /api/v1/receiver/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteReceiverCommand(id);
        return await HandleDeleteAsync(command, id, EntityName);
    }
}
