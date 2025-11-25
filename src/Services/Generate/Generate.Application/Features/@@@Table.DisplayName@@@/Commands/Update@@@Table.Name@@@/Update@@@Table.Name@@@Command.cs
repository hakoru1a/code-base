@@@copyRight@@@
using MediatR;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.DisplayName@@@.Commands.Update @@@Table.DisplayName@@@
{
    /// <summary>
    /// Command to update an existing @@@Table.DisplayName@@@
    /// </summary>
    public class Update@@@Table.DisplayName @@@Command : IRequest<Result<bool>>
    {
<FOREACH>
    <LOOP>TableGenerate.Columns</LOOP>
    <CONTENT>        public ###SourceCodeType### ###ColumnName### { get; set; }</CONTENT>
</FOREACH>
    }
}