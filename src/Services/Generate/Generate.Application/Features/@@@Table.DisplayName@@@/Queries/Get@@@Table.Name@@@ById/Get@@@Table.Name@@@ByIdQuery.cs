@@@copyRight@@@
using MediatR;
using Shared.DTOs.@@@Table.DisplayName@@@;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.DisplayName@@@.Queries.Get @@@Table.DisplayName@@@ById
{
    /// <summary>
    /// Query to get @@@Table.DisplayName@@@ by ID
    /// </summary>
    public class Get@@@Table.DisplayName @@@ByIdQuery : IRequest<Result<@@@Table.DisplayName@@@ResponseDto>>
    {
        public long Id { get; set; }
}
}