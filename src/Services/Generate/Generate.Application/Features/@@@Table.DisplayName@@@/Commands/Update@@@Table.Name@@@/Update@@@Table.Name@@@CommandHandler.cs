@@@copyRight@@@
using Generate.Application.Contracts.Persistence;
using Generate.Domain.Entities;
using Mapster;
using MediatR;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.DisplayName@@@.Commands.Update @@@Table.DisplayName@@@
{
    /// <summary>
    /// Handler for Update@@@Table.DisplayName@@@Command
    /// </summary>
    public class Update@@@Table.DisplayName @@@CommandHandler : IRequestHandler<Update@@@Table.DisplayName@@@Command, Result<bool>>
    {
        private readonly I @@@Table.DisplayName@@@Repository _repository;

public Update @@@Table.DisplayName@@@CommandHandler(I @@@Table.DisplayName @@@Repository repository)
{
    _repository = repository;
}

public async Task<Result<bool>> Handle(Update @@@Table.DisplayName @@@Command request, CancellationToken cancellationToken)
{
    var entity = await _repository.GetByIdAsync(request.Id);
    if (entity == null)
    {
        return Result<bool>.Failure("@@@Table.DisplayName@@@ not found");
    }

    request.Adapt(entity);
    await _repository.UpdateAsync(entity);

    return Result<bool>.Success(true);
}
}
}