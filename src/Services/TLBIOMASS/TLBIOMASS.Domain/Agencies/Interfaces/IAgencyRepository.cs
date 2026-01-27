using Contracts.Common.Interface;
using TLBIOMASS.Domain.Agencies;

namespace TLBIOMASS.Domain.Agencies.Interfaces;

public interface IAgencyRepository : IRepositoryBaseAsync<Agency, int>
{
}
