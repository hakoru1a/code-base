@@@copyRight@@@
using Contracts.Common.Interface;
using Generate.Domain.Entities;

namespace Generate.Infrastructure.Interfaces
{
    public interface I@@@Table.Name@@@Repository : IRepositoryBaseAsync<@@@Table.Name@@@, long>
    {
    }
}