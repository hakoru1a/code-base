using Contracts.Common.Interface;
using TLBIOMASS.Domain.BankAccounts;
using Shared.Domain.Enums;

namespace TLBIOMASS.Domain.BankAccounts.Interfaces;

public interface IBankAccountRepository : IRepositoryBaseAsync<BankAccount, int>
{
    Task<IEnumerable<BankAccount>> GetByOwnerAsync(OwnerType ownerType, int ownerId);
    Task<BankAccount?> GetDefaultByOwnerAsync(OwnerType ownerType, int ownerId);
}
