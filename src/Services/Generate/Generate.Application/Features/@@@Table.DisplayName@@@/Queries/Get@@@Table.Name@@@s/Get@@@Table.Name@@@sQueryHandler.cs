@@@copyRight@@@
using Generate.Application.Contracts.Persistence;
using Generate.Domain.Entities;
using Mapster;
using MediatR;
using Shared.DTOs.@@@Table.DisplayName@@@;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.DisplayName@@@.Queries.Get @@@Table.DisplayName@@@s
{
    /// <summary>
    /// Handler for Get@@@Table.DisplayName@@@sQuery
    /// </summary>
    public class Get@@@Table.DisplayName @@@sQueryHandler : IRequestHandler<Get@@@Table.DisplayName@@@sQuery, Result<List<@@@Table.DisplayName@@@ResponseDto>>>
    {
        private readonly I @@@Table.DisplayName@@@Repository _repository;

public Get @@@Table.DisplayName@@@sQueryHandler(I @@@Table.DisplayName @@@Repository repository)
{
    _repository = repository;
}

public async Task<Result<List<@@@Table.DisplayName@@@ResponseDto>>> Handle(Get @@@Table.DisplayName @@@sQuery request, CancellationToken cancellationToken)
{
    var entities = await _repository.GetAllAsync();
    var dtos = entities.Adapt < List < @@@Table.DisplayName@@@ResponseDto >> ();

    return Result < List < @@@Table.DisplayName@@@ResponseDto >>.Success(dtos);
}
}
}