using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Common;
using Shared.SeedWork;
using TLBIOMASS.Application.Features.Payments.Commands.SetWeighingTicketPayment;
using TLBIOMASS.Application.Features.Payments.Commands.CreatePayment;
using TLBIOMASS.Application.Features.Payments.Commands.UpdatePaymentGroupStatus;
using TLBIOMASS.Application.Features.Payments.Queries.GetPaymentGroups;
using TLBIOMASS.Application.Features.Payments.Queries.GetPaymentDetails;
using TLBIOMASS.Application.Features.Payments.Queries.GetPaymentDetailById;
using TLBIOMASS.Application.Features.Payments.Queries.GetPaymentDetailsByCode;
using Shared.DTOs.Payment;

namespace TLBIOMASS.API.Controllers;

[ApiVersion("1.0")]
public class PaymentController : ApiControllerBase<PaymentController>
{
    private const string EntityName = "Payment";

    public PaymentController(IMediator mediator, ILogger<PaymentController> logger)
        : base(mediator, logger)
    {
    }

    [HttpPatch("group-status")]
    [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateGroupStatus([FromBody] UpdatePaymentGroupStatusCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(new ApiSuccessResult<bool>(result, "Payment group status updated successfully."));
    }

    [HttpGet("groups")]
    [ProducesResponseType(typeof(ApiSuccessResult<PagedList<PaymentGroupResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetGroups([FromQuery] PaymentDetailFilterDto filter)
    {
        var query = new GetPaymentGroupsQuery { Filter = filter };
        return await HandleGetPagedAsync<GetPaymentGroupsQuery, PaymentGroupResponseDto>(
            query, "PaymentGroup", filter.PageNumber, filter.PageSize);
    }

    [HttpGet("details")]
    [ProducesResponseType(typeof(ApiSuccessResult<PagedList<PaymentDetailResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDetails([FromQuery] PaymentDetailFilterDto filter)
    {
        var query = new GetPaymentDetailsQuery { Filter = filter };
        return await HandleGetPagedAsync<GetPaymentDetailsQuery, PaymentDetailResponseDto>(
            query, "PaymentDetail", filter.PageNumber, filter.PageSize);
    }

    /// <summary>
    /// Gets all payment details for a specific payment code (Group Details)
    /// </summary>
    [HttpGet("groups/{code}")]
    [ProducesResponseType(typeof(ApiSuccessResult<List<PaymentDetailResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCode([FromRoute] string code)
    {
        var query = new GetPaymentDetailsByCodeQuery(code);
        return await HandleGetAllAsync<GetPaymentDetailsByCodeQuery, PaymentDetailResponseDto>(query, "PaymentGroupDetails");
    }

    /// <summary>
    /// Gets a single payment detail by ID
    /// </summary>
    [HttpGet("details/{id}")]
    [ProducesResponseType(typeof(ApiSuccessResult<PaymentDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult<PaymentDetailResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var query = new GetPaymentDetailByIdQuery(id);
        return await HandleGetByIdAsync<GetPaymentDetailByIdQuery, PaymentDetailResponseDto>(query, id, "PaymentDetail");
    }

    [HttpPost("config")]
    [ProducesResponseType(typeof(ApiSuccessResult<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetConfig([FromBody] SetWeighingTicketPaymentCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(new ApiSuccessResult<int>(result, "Payment configuration set successfully."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiSuccessResult<List<PaymentResultDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(new ApiSuccessResult<List<PaymentResultDto>>(result, "Payments created successfully."));
    }
}
