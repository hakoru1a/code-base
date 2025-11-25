@@@copyRight@@@
using Generate.Application.Contracts.Persistence;
using Generate.Domain.Entities;
using Mapster;
using MediatR;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.DisplayName@@@.Commands.Create @@@Table.DisplayName@@@
{
    /// <summary>
    /// Handler for Create@@@Table.DisplayName@@@Command
    /// </summary>
    public class Create@@@Table.DisplayName @@@CommandHandler : IRequestHandler<Create@@@Table.DisplayName@@@Command, Result<long>>
    {
        private readonly I @@@Table.DisplayName@@@Repository _repository;

public Create @@@Table.DisplayName@@@CommandHandler(I @@@Table.DisplayName @@@Repository repository)
{
    _repository = repository;
}

public async Task<Result<long>> Handle(Create @@@Table.DisplayName @@@Command request, CancellationToken cancellationToken)
{
    var entity = request.Adapt < @@@Table.DisplayName@@@> ();
    var result = await _repository.AddAsync(entity);

    return Result<long>.Success(result.Id);
}
}
}