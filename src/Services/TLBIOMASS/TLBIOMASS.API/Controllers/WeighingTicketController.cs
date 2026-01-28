using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Common;
using Shared.SeedWork;
using Shared.DTOs.WeighingTicket;
using TLBIOMASS.Application.Features.WeighingTickets.Queries.GetAllWeighingTickets;
using TLBIOMASS.Application.Features.WeighingTickets.Queries.GetWeighingTickets;
using TLBIOMASS.Application.Features.WeighingTickets.Queries.GetWeighingTicketById;
using TLBIOMASS.Application.Features.WeighingTickets.Commands.AssignReceiver;

namespace TLBIOMASS.API.Controllers;

[ApiVersion("1.0")]
public class WeighingTicketController : ApiControllerBase<WeighingTicketController>
{
    private const string EntityName = "WeighingTicket";

    public WeighingTicketController(IMediator mediator, ILogger<WeighingTicketController> logger)
        : base(mediator, logger)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResult<List<WeighingTicketResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetList([FromQuery] WeighingTicketFilterDto filter)
    {
        var query = new GetAllWeighingTicketsQuery { Filter = filter };
        return await HandleGetAllAsync<GetAllWeighingTicketsQuery, WeighingTicketResponseDto>(query, EntityName);
    }

    [HttpGet("paged")]
    [ProducesResponseType(typeof(ApiSuccessResult<List<WeighingTicketResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPagedList([FromQuery] WeighingTicketPagedFilterDto filter)
    {
        var query = new GetWeighingTicketsQuery { Filter = filter };
        return await HandleGetPagedAsync<GetWeighingTicketsQuery, WeighingTicketResponseDto>(
            query, EntityName, filter.PageNumber, filter.PageSize);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiSuccessResult<WeighingTicketResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult<WeighingTicketResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetWeighingTicketByIdQuery(id);
        return await HandleGetByIdAsync<GetWeighingTicketByIdQuery, WeighingTicketResponseDto>(query, id, EntityName);
    }

    [HttpPatch("{id}/receiver")]
    [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignReceiver(int id, [FromBody] AssignReceiverDto request)
    {
        var command = new AssignReceiverCommand 
        { 
            WeighingTicketId = id, 
            ReceiverId = request.ReceiverId 
        };
        var result = await Mediator.Send(command);
        return Ok(new ApiSuccessResult<bool>(result, "Receiver assigned successfully."));
    }
}
