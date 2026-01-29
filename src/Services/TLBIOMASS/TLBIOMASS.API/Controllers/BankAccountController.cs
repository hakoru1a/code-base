using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.BankAccount;
using Shared.SeedWork;
using TLBIOMASS.Application.Features.BankAccounts.Commands.CreateBankAccount;
using TLBIOMASS.Application.Features.BankAccounts.Commands.DeleteBankAccount;
using TLBIOMASS.Application.Features.BankAccounts.Commands.SetDefaultBankAccount;
using TLBIOMASS.Application.Features.BankAccounts.Commands.UpdateBankAccount;
using TLBIOMASS.Application.Features.BankAccounts.Queries.GetBankAccountById;
using TLBIOMASS.Application.Features.BankAccounts.Queries.GetBankAccountsByOwner;
using Mapster;
using Infrastructure.Common;

namespace TLBIOMASS.API.Controllers;

[ApiVersion("1.0")]
public class BankAccountController : ApiControllerBase<BankAccountController>
{
    private const string EntityName = "BankAccount";

    public BankAccountController(IMediator mediator, ILogger<BankAccountController> logger)
        : base(mediator, logger)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResult<IEnumerable<BankAccountResponseDto>>), 200)]
    public async Task<IActionResult> GetByOwner([FromQuery] string ownerType, [FromQuery] int ownerId)
    {
        var query = new GetBankAccountsByOwnerQuery { OwnerType = ownerType, OwnerId = ownerId };
        return await HandleGetAllAsync<GetBankAccountsByOwnerQuery, BankAccountResponseDto>(query, EntityName);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiSuccessResult<BankAccountResponseDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetBankAccountByIdQuery { Id = id };
        return await HandleGetByIdAsync<GetBankAccountByIdQuery, BankAccountResponseDto>(query, id, EntityName);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiSuccessResult<int>), 201)]
    public async Task<IActionResult> Create([FromBody] BankAccountCreateDto dto)
    {
        var command = new CreateBankAccountCommand { Data = dto };
        return await HandleCreateAsync(command, EntityName, dto.AccountNumber);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiSuccessResult<bool>), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] BankAccountUpdateDto dto)
    {
        var command = new UpdateBankAccountCommand { Id = id, Data = dto };
        return await HandleUpdateAsync(command, id, id, EntityName);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiSuccessResult<bool>), 200)]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteBankAccountCommand { Id = id };
        return await HandleDeleteAsync(command, id, EntityName);
    }

    [HttpPut("{id}/set-default")]
    [ProducesResponseType(typeof(ApiSuccessResult<bool>), 200)]
    public async Task<IActionResult> SetDefault(int id)
    {
        var command = new SetDefaultBankAccountCommand { Id = id };
        return await HandleUpdateAsync(command, id, id, EntityName);
    }
}
