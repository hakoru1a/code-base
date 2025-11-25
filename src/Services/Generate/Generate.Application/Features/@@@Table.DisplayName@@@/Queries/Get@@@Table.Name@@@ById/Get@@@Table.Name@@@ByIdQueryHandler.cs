@@@copyRight@@@
using Generate.Application.Contracts.Persistence;
using Generate.Domain.Entities;
using Mapster;
using MediatR;
using Shared.DTOs.@@@Table.DisplayName@@@;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.DisplayName@@@.Queries.Get @@@Table.DisplayName@@@ById
{
    /// <summary>
    /// Handler for Get@@@Table.DisplayName@@@ByIdQuery
    /// </summary>
    public class Get@@@Table.DisplayName @@@ByIdQueryHandler : IRequestHandler<Get@@@Table.DisplayName@@@ByIdQuery, Result<@@@Table.DisplayName@@@ResponseDto>>
    {
        private readonly I @@@Table.DisplayName@@@Repository _repository;

public Get @@@Table.DisplayName@@@ByIdQueryHandler(I @@@Table.DisplayName @@@Repository repository)
{
    _repository = repository;
}

public async Task<Result<@@@Table.DisplayName@@@ResponseDto>> Handle(Get @@@Table.DisplayName @@@ByIdQuery request, CancellationToken cancellationToken)
{
    var entity = await _repository.GetByIdAsync(request.Id);
    if (entity == null)
    {
        return Result < @@@Table.DisplayName@@@ResponseDto >.Failure("@@@Table.DisplayName@@@ not found");
    }

    var dto = entity.Adapt < @@@Table.DisplayName@@@ResponseDto > ();
    return Result < @@@Table.DisplayName@@@ResponseDto >.Success(dto);
}
}
}