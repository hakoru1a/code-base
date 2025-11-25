@@@copyRight@@@
using Contracts.Common.Interface;
using Generate.Domain.Entities;
using Generate.Application.Contracts.Persistence;
using Generate.Infrastructure.Persistences;
using Infrastructure.Common.Repository;

namespace Generate.Infrastructure.Repositories
{
    public class @@@Table.Name@@@Repository : RepositoryBaseAsync<@@@Table.Name@@@, long, GenerateContext>, I@@@Table.Name@@@Repository
    {
        public @@@Table.Name@@@Repository(GenerateContext dbContext, IUnitOfWork<GenerateContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }
    }
}