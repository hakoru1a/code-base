@@@copyRight@@@
using Contracts.Common.Interface;
using Generate.Domain.Entities;
using Generate.Application.Contracts.Persistence;
using Generate.Infrastructure.Persistences;
using Infrastructure.Common.Repository;

namespace Generate.Infrastructure.Repositories
{
    public class @@@Table.DisplayName @@@Repository : RepositoryBaseAsync<@@@Table.DisplayName@@@, long, GenerateContext>, I @@@Table.DisplayName@@@Repository
    {
        public @@@Table.DisplayName @@@Repository(GenerateContext dbContext, IUnitOfWork<GenerateContext> unitOfWork) : base(dbContext, unitOfWork)
        {
    }
}
}