@@@copyRight@@@
using Generate.Application.Contracts.Persistence;
using Generate.Domain.Entities;
using Mapster;
using MediatR;
using Shared.DTOs.@@@Table.Name@@@;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.Name@@@.Queries.Get@@@Table.Name@@@s
{
    /// <summary>
    /// Handler for Get@@@Table.Name@@@sQuery
    /// </summary>
    public class Get@@@Table.Name@@@sQueryHandler : IRequestHandler<Get@@@Table.Name@@@sQuery, Result<List<@@@Table.Name@@@ResponseDto>>>
    {
        private readonly I@@@Table.Name@@@Repository _repository;

        public Get@@@Table.Name@@@sQueryHandler(I@@@Table.Name@@@Repository repository)
        {
            _repository = repository;
        }

        public async Task<Result<List<@@@Table.Name@@@ResponseDto>>> Handle(Get@@@Table.Name@@@sQuery request, CancellationToken cancellationToken)
        {
            var entities = await _repository.GetAllAsync();
            var dtos = entities.Adapt<List<@@@Table.Name@@@ResponseDto>>();
            
            return Result<List<@@@Table.Name@@@ResponseDto>>.Success(dtos);
        }
    }
}