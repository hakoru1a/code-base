using Asp.Versioning;
using Infrastructure.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Identity;
using Shared.SeedWork;
using TLBIOMASS.Application.Features.Customers.Commands.CreateCustomer;
using TLBIOMASS.Application.Features.Customers.Commands.DeleteCustomer;
using TLBIOMASS.Application.Features.Customers.Commands.UpdateCustomer;
using TLBIOMASS.Application.Features.Customers.DTOs;
using TLBIOMASS.Application.Features.Customers.Queries.GetAllCustomers;
using TLBIOMASS.Application.Features.Customers.Queries.GetCustomerById;
using TLBIOMASS.Application.Features.Customers.Queries.GetCustomers;

namespace TLBIOMASS.API.Controllers;

[ApiVersion("1.0")]
// [Authorize]
public class CustomerController : ApiControllerBase<CustomerController>
{
    private const string EntityName = "Customer";

    public CustomerController(IMediator mediator, ILogger<CustomerController> logger)
        : base(mediator, logger)
    {
    }

    [HttpGet]
    // [Authorize(Policy = PolicyNames.TLBIOMASS.Customer.CanView)]
    [ProducesResponseType(typeof(ApiSuccessResult<PagedList<CustomerResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] string? sortDirection = "desc")
    {
        var query = new GetCustomersQuery
        {
            Page = page,
            Size = size,
            Search = search,
            IsActive = isActive,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        return await HandleGetPagedAsync<GetCustomersQuery, CustomerResponseDto>(
            query, EntityName, page, size);
    }

  
    [HttpGet("all")]
    // [Authorize(Policy = PolicyNames.TLBIOMASS.Customer.CanView)]
    [ProducesResponseType(typeof(ApiSuccessResult<List<CustomerResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] string? sortDirection = "desc")
    {
        var query = new GetAllCustomersQuery
        {
            Search = search,
            IsActive = isActive,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        return await HandleGetAllAsync<GetAllCustomersQuery, CustomerResponseDto>(query, EntityName);
    }

    [HttpGet("{id}")]
    // [Authorize(Policy = PolicyNames.TLBIOMASS.Customer.CanView)]
    [ProducesResponseType(typeof(ApiSuccessResult<CustomerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult<CustomerResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetCustomerByIdQuery { Id = id };
        return await HandleGetByIdAsync<GetCustomerByIdQuery, CustomerResponseDto>(query, id, EntityName);
    }

    [HttpPost]
    // [Authorize(Policy = PolicyNames.TLBIOMASS.Customer.CanCreate)]
    [ProducesResponseType(typeof(ApiSuccessResult<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CustomerCreateDto dto)
    {
        var command = new CreateCustomerCommand
        {
            TenKhachHang = dto.TenKhachHang,
            DienThoai = dto.DienThoai,
            DiaChi = dto.DiaChi,
            Email = dto.Email,
            MaSoThue = dto.MaSoThue,
            GhiChu = dto.GhiChu
        };

        return await HandleCreateAsync(command, EntityName, dto.TenKhachHang);
    }

    [HttpPut("{id}")]
    // [Authorize(Policy = PolicyNames.TLBIOMASS.Customer.CanUpdate)]
    [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] CustomerUpdateDto dto)
    {
        var command = new UpdateCustomerCommand
        {
            Id = dto.Id,
            TenKhachHang = dto.TenKhachHang,
            DienThoai = dto.DienThoai,
            DiaChi = dto.DiaChi,
            Email = dto.Email,
            MaSoThue = dto.MaSoThue,
            GhiChu = dto.GhiChu,
            IsActive = dto.IsActive
        };

        return await HandleUpdateAsync(command, id, dto.Id, EntityName);
    }

    [HttpDelete("{id}")]
    // [Authorize(Policy = PolicyNames.TLBIOMASS.Customer.CanDelete)]
    [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteCustomerCommand { Id = id };
        return await HandleDeleteAsync(command, id, EntityName);
    }

    [HttpPatch("{id}/status")]
    // [Authorize(Policy = PolicyNames.TLBIOMASS.Customer.CanUpdate)]
    [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateStatus(int id, [FromQuery] bool isActive)
    {
        Logger.LogInformation("Updating status of {EntityName} with ID: {Id} to {IsActive}", 
            EntityName, id, isActive);

        // Get current customer to populate the command correctly
        var getQuery = new GetCustomerByIdQuery { Id = id };
        var customer = await Mediator.Send(getQuery);

        if (customer == null)
        {
            return NotFound(new ApiErrorResult<bool>($"Không tìm thấy {EntityName} với ID: {id}"));
        }

        var command = new UpdateCustomerCommand
        {
            Id = id,
            TenKhachHang = customer.TenKhachHang,
            DienThoai = customer.DienThoai,
            DiaChi = customer.DiaChi,
            Email = customer.Email,
            MaSoThue = customer.MaSoThue,
            GhiChu = customer.GhiChu,
            IsActive = isActive
        };

        var result = await Mediator.Send(command);
        var statusText = isActive ? "kích hoạt" : "vô hiệu hóa";
        
        return Ok(new ApiSuccessResult<bool>(result, $"{EntityName} đã được {statusText} thành công"));
    }
}
