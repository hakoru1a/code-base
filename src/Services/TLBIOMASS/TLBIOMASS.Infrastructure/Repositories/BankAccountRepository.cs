using Contracts.Common.Interface;
using Infrastructure.Common.Repository;
using Microsoft.EntityFrameworkCore;
using TLBIOMASS.Domain.BankAccounts;
using TLBIOMASS.Domain.BankAccounts.Interfaces;
using TLBIOMASS.Infrastructure.Persistences;

namespace TLBIOMASS.Infrastructure.Repositories;

public class BankAccountRepository : RepositoryBaseAsync<BankAccount, int, TLBIOMASSContext>, IBankAccountRepository
{
    public BankAccountRepository(TLBIOMASSContext dbContext, IUnitOfWork<TLBIOMASSContext> unitOfWork) 
        : base(dbContext, unitOfWork)
    {
    }

    public async Task<IEnumerable<BankAccount>> GetByOwnerAsync(string ownerType, int ownerId)
    {
        return await FindAll()
            .Where(x => x.OwnerType == ownerType && x.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<BankAccount?> GetDefaultByOwnerAsync(string ownerType, int ownerId)
    {
        return await FindAll()
            .FirstOrDefaultAsync(x => x.OwnerType == ownerType && x.OwnerId == ownerId && x.IsDefault);
    }
}
