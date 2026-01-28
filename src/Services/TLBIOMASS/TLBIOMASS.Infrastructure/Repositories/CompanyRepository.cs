using Contracts.Common.Interface;
using Infrastructure.Common.Repository;
using TLBIOMASS.Domain.Companies;
using TLBIOMASS.Domain.Companies.Interfaces;
using TLBIOMASS.Infrastructure.Persistences;

namespace TLBIOMASS.Infrastructure.Repositories;

public class CompanyRepository : RepositoryBaseAsync<Company, int, TLBIOMASSContext>, ICompanyRepository
{
    public CompanyRepository(TLBIOMASSContext dbContext, IUnitOfWork<TLBIOMASSContext> unitOfWork) 
        : base(dbContext, unitOfWork)
    {
    }
}
