@@@copyRight@@@
using Generate.Application.Contracts.Persistence;
using MediatR;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.Name@@@.Commands.Delete@@@Table.Name@@@
{
    /// <summary>
    /// Handler for Delete@@@Table.Name@@@Command
    /// </summary>
    public class Delete@@@Table.Name@@@CommandHandler : IRequestHandler<Delete@@@Table.Name@@@Command, Result<bool>>
    {
        private readonly I@@@Table.Name@@@Repository _repository;

        public Delete@@@Table.Name@@@CommandHandler(I@@@Table.Name@@@Repository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(Delete@@@Table.Name@@@Command request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id);
            if (entity == null)
            {
                return Result<bool>.Failure("@@@Table.Name@@@ not found");
            }

            await _repository.DeleteAsync(entity);
            return Result<bool>.Success(true);
        }
    }
}