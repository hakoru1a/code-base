using Asp.Versioning;
using Infrastructure.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Customer;
using Shared.SeedWork;
using TLBIOMASS.Application.Features.Customers.Commands.CreateCustomer;
using TLBIOMASS.Application.Features.Customers.Commands.DeleteCustomer;
using TLBIOMASS.Application.Features.Customers.Commands.UpdateCustomer;
using TLBIOMASS.Application.Features.Customers.Queries.GetCustomerById;
using TLBIOMASS.Application.Features.Customers.Queries.GetCustomers;
using TLBIOMASS.Application.Features.Customers.Queries.GetCustomersPaged;
using Mapster;

namespace TLBIOMASS.API.Controllers;

[ApiVersion("1.0")]
public class CustomerController : ApiControllerBase<CustomerController>
{
    private const string EntityName = "Customer";

    public CustomerController(IMediator mediator, ILogger<CustomerController> logger)
        : base(mediator, logger)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResult<List<CustomerResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetList([FromQuery] CustomerFilterDto filter)
    {
        var query = new GetCustomersQuery { Filter = filter };
        return await HandleGetAllAsync<GetCustomersQuery, CustomerResponseDto>(query, EntityName);
    }

    [HttpGet("paged")]
    [ProducesResponseType(typeof(ApiSuccessResult<PagedList<CustomerResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPagedList([FromQuery] CustomerPagedFilterDto filter)
    {
        var query = new GetCustomersPagedQuery { Filter = filter };
        return await HandleGetPagedAsync<GetCustomersPagedQuery, CustomerResponseDto>(
            query, EntityName, filter.PageNumber, filter.PageSize);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiSuccessResult<CustomerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult<CustomerResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetCustomerByIdQuery { Id = id };
        return await HandleGetByIdAsync<GetCustomerByIdQuery, CustomerResponseDto>(query, id, EntityName);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiSuccessResult<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CustomerCreateDto dto)
    {
        var command = dto.Adapt<CreateCustomerCommand>();
        return await HandleCreateAsync(command, EntityName, dto.Name);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] CustomerUpdateDto dto)
    {
        var command = dto.Adapt<UpdateCustomerCommand>();
        command.Id = id;
        return await HandleUpdateAsync(command, id, dto.Id, EntityName);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteCustomerCommand { Id = id };
        return await HandleDeleteAsync(command, id, EntityName);
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateStatus(int id, [FromQuery] bool isActive)
    {
        var getQuery = new GetCustomerByIdQuery { Id = id };
        var customer = await Mediator.Send(getQuery);

        if (customer == null)
        {
            return NotFound(new ApiErrorResult<bool>(ResponseMessages.ItemNotFound(EntityName, id)));
        }

        var command = customer.Adapt<UpdateCustomerCommand>();
        command.IsActive = isActive;

        var result = await Mediator.Send(command);
        return Ok(new ApiSuccessResult<bool>(result, ResponseMessages.UpdateSuccess));
    }
}
