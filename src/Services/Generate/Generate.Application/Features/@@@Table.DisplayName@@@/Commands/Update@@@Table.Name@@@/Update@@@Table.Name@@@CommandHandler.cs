@@@copyRight@@@
using Generate.Application.Contracts.Persistence;
using Generate.Domain.Entities;
using Mapster;
using MediatR;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.Name@@@.Commands.Update@@@Table.Name@@@
{
    /// <summary>
    /// Handler for Update@@@Table.Name@@@Command
    /// </summary>
    public class Update@@@Table.Name@@@CommandHandler : IRequestHandler<Update@@@Table.Name@@@Command, Result<bool>>
    {
        private readonly I@@@Table.Name@@@Repository _repository;

        public Update@@@Table.Name@@@CommandHandler(I@@@Table.Name@@@Repository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(Update@@@Table.Name@@@Command request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id);
            if (entity == null)
            {
                return Result<bool>.Failure("@@@Table.Name@@@ not found");
            }

            request.Adapt(entity);
            await _repository.UpdateAsync(entity);
            
            return Result<bool>.Success(true);
        }
    }
}