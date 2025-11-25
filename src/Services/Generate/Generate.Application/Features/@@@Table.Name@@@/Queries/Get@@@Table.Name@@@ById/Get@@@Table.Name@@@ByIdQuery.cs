@@@copyRight@@@
using MediatR;
using Shared.DTOs.@@@Table.Name@@@;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.Name@@@.Queries.Get@@@Table.Name@@@ById
{
    /// <summary>
    /// Query to get @@@Table.Name@@@ by ID
    /// </summary>
    public class Get@@@Table.Name@@@ByIdQuery : IRequest<Result<@@@Table.Name@@@ResponseDto>>
    {
        public long Id { get; set; }
    }
}