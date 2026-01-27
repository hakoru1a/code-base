using Contracts.Common.Interface;
using TLBIOMASS.Domain.Landowners;

namespace TLBIOMASS.Domain.Landowners.Interfaces;

public interface ILandownerRepository : IRepositoryBaseAsync<Landowner, int>
{
}
