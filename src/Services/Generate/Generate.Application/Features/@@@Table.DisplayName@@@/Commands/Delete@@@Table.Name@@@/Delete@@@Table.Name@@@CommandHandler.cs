@@@copyRight@@@
using Generate.Application.Contracts.Persistence;
using MediatR;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.DisplayName@@@.Commands.Delete @@@Table.DisplayName@@@
{
    /// <summary>
    /// Handler for Delete@@@Table.DisplayName@@@Command
    /// </summary>
    public class Delete@@@Table.DisplayName @@@CommandHandler : IRequestHandler<Delete@@@Table.DisplayName@@@Command, Result<bool>>
    {
        private readonly I @@@Table.DisplayName@@@Repository _repository;

public Delete @@@Table.DisplayName@@@CommandHandler(I @@@Table.DisplayName @@@Repository repository)
{
    _repository = repository;
}

public async Task<Result<bool>> Handle(Delete @@@Table.DisplayName @@@Command request, CancellationToken cancellationToken)
{
    var entity = await _repository.GetByIdAsync(request.Id);
    if (entity == null)
    {
        return Result<bool>.Failure("@@@Table.DisplayName@@@ not found");
    }

    await _repository.DeleteAsync(entity);
    return Result<bool>.Success(true);
}
}
}