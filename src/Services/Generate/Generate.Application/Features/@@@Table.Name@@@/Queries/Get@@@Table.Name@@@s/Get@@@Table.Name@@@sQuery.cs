@@@copyRight@@@
using MediatR;
using Shared.DTOs.@@@Table.Name@@@;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.Name@@@.Queries.Get@@@Table.Name@@@s
{
    /// <summary>
    /// Query to get all @@@Table.Name@@@s
    /// </summary>
    public class Get@@@Table.Name@@@sQuery : IRequest<Result<List<@@@Table.Name@@@ResponseDto>>>
    {
    }
}