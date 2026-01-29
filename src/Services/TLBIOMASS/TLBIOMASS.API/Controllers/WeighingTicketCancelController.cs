using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Common;
using Shared.SeedWork;
using TLBIOMASS.Application.Features.WeighingTicketCancels.Commands.CreateWeighingTicketCancel;
using TLBIOMASS.Application.Features.WeighingTicketCancels.Commands.DeleteWeighingTicketCancel;
using Shared.DTOs.WeighingTicketCancel;
using TLBIOMASS.Application.Features.WeighingTicketCancels.Queries.GetWeighingTicketCancels;
using TLBIOMASS.Application.Features.WeighingTicketCancels.Queries.GetWeighingTicketCancelById;

namespace TLBIOMASS.API.Controllers;

[ApiVersion("1.0")]
public class WeighingTicketCancelController : ApiControllerBase<WeighingTicketCancelController>
{
    private const string EntityName = "WeighingTicketCancel";

    public WeighingTicketCancelController(IMediator mediator, ILogger<WeighingTicketCancelController> logger)
        : base(mediator, logger)
    {
    }

    [HttpGet("paged")]
    [ProducesResponseType(typeof(ApiSuccessResult<List<WeighingTicketCancelResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPagedList([FromQuery] WeighingTicketCancelPagedFilterDto filter)
    {
        var query = new GetWeighingTicketCancelsQuery { Filter = filter };
        return await HandleGetPagedAsync<GetWeighingTicketCancelsQuery, WeighingTicketCancelResponseDto>(
            query, EntityName, filter.PageNumber, filter.PageSize);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiSuccessResult<WeighingTicketCancelResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult<WeighingTicketCancelResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetWeighingTicketCancelByIdQuery(id);
        return await HandleGetByIdAsync<GetWeighingTicketCancelByIdQuery, WeighingTicketCancelResponseDto>(query, id, EntityName);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiSuccessResult<long>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateWeighingTicketCancelCommand command)
    {
        // No adaptor needed as command is simple properties
        return await HandleCreateAsync(command, EntityName, command.WeighingTicketId.ToString());
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteWeighingTicketCancelCommand(id);
        return await HandleDeleteAsync(command, id, EntityName);
    }
}
