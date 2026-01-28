using Contracts.Common.Interface;
using TLBIOMASS.Domain.Companies;

namespace TLBIOMASS.Domain.Companies.Interfaces;

public interface ICompanyRepository : IRepositoryBaseAsync<Company, int>
{
}
