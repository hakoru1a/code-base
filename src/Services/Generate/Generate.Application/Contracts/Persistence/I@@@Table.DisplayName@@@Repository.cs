@@@copyRight@@@
using Contracts.Common.Interface;
using Generate.Domain.Entities;

namespace Generate.Application.Contracts.Persistence
{
    public interface I@@@Table.Name@@@Repository : IRepositoryBaseAsync<@@@Table.Name@@@, long>
    {
    }
}