using Contracts.Common.Interface;
using TLBIOMASS.Domain.BankAccounts;

namespace TLBIOMASS.Domain.BankAccounts.Interfaces;

public interface IBankAccountRepository : IRepositoryBaseAsync<BankAccount, int>
{
    Task<IEnumerable<BankAccount>> GetByOwnerAsync(string ownerType, int ownerId);
    Task<BankAccount?> GetDefaultByOwnerAsync(string ownerType, int ownerId);
}
