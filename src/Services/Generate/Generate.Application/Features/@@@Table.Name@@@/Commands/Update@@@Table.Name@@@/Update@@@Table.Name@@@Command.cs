@@@copyRight@@@
using MediatR;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.Name@@@.Commands.Update@@@Table.Name@@@
{
    /// <summary>
    /// Command to update an existing @@@Table.Name@@@
    /// </summary>
    public class Update@@@Table.Name@@@Command : IRequest<Result<bool>>
    {
<FOREACH>
    <LOOP>TableGenerate.Columns</LOOP>
    <CONTENT>        public ###ConvertTypeDbToCode### ###Name### { get; set; }</CONTENT>
</FOREACH>
    }
}