@@@copyRight@@@
using MediatR;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.Name@@@.Commands.Delete@@@Table.Name@@@
{
    /// <summary>
    /// Command to delete a @@@Table.Name@@@
    /// </summary>
    public class Delete@@@Table.Name@@@Command : IRequest<Result<bool>>
    {
        public long Id { get; set; }
    }
}