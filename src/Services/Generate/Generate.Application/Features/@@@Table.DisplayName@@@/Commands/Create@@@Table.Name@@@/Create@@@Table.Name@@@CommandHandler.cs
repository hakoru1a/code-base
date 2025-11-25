@@@copyRight@@@
using Generate.Application.Contracts.Persistence;
using Generate.Domain.Entities;
using Mapster;
using MediatR;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.Name@@@.Commands.Create@@@Table.Name@@@
{
    /// <summary>
    /// Handler for Create@@@Table.Name@@@Command
    /// </summary>
    public class Create@@@Table.Name@@@CommandHandler : IRequestHandler<Create@@@Table.Name@@@Command, Result<long>>
    {
        private readonly I@@@Table.Name@@@Repository _repository;

        public Create@@@Table.Name@@@CommandHandler(I@@@Table.Name@@@Repository repository)
        {
            _repository = repository;
        }

        public async Task<Result<long>> Handle(Create@@@Table.Name@@@Command request, CancellationToken cancellationToken)
        {
            var entity = request.Adapt<@@@Table.Name@@@>();
            var result = await _repository.AddAsync(entity);
            
            return Result<long>.Success(result.Id);
        }
    }
}