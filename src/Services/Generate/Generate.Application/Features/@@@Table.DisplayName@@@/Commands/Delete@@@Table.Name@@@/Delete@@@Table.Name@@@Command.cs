@@@copyRight@@@
using MediatR;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.DisplayName@@@.Commands.Delete @@@Table.DisplayName@@@
{
    /// <summary>
    /// Command to delete a @@@Table.DisplayName@@@
    /// </summary>
    public class Delete@@@Table.DisplayName @@@Command : IRequest<Result<bool>>
    {
        public long Id { get; set; }
}
}