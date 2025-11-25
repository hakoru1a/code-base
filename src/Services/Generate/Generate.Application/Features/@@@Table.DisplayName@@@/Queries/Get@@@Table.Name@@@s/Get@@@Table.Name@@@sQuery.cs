@@@copyRight@@@
using MediatR;
using Shared.DTOs.@@@Table.DisplayName@@@;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.DisplayName@@@.Queries.Get @@@Table.DisplayName@@@s
{
    /// <summary>
    /// Query to get all @@@Table.DisplayName@@@s
    /// </summary>
    public class Get@@@Table.DisplayName @@@sQuery : IRequest<Result<List<@@@Table.DisplayName@@@ResponseDto>>>
    {
    }
}