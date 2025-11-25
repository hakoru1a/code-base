@@@copyRight@@@
using Generate.Application.Contracts.Persistence;
using Generate.Domain.Entities;
using Mapster;
using MediatR;
using Shared.DTOs.@@@Table.Name@@@;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.Name@@@.Queries.Get@@@Table.Name@@@ById
{
    /// <summary>
    /// Handler for Get@@@Table.Name@@@ByIdQuery
    /// </summary>
    public class Get@@@Table.Name@@@ByIdQueryHandler : IRequestHandler<Get@@@Table.Name@@@ByIdQuery, Result<@@@Table.Name@@@ResponseDto>>
    {
        private readonly I@@@Table.Name@@@Repository _repository;

        public Get@@@Table.Name@@@ByIdQueryHandler(I@@@Table.Name@@@Repository repository)
        {
            _repository = repository;
        }

        public async Task<Result<@@@Table.Name@@@ResponseDto>> Handle(Get@@@Table.Name@@@ByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id);
            if (entity == null)
            {
                return Result<@@@Table.Name@@@ResponseDto>.Failure("@@@Table.Name@@@ not found");
            }

            var dto = entity.Adapt<@@@Table.Name@@@ResponseDto>();
            return Result<@@@Table.Name@@@ResponseDto>.Success(dto);
        }
    }
}