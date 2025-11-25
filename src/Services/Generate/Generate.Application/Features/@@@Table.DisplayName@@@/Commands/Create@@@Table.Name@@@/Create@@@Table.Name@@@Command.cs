@@@copyRight@@@
using MediatR;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.DisplayName@@@.Commands.Create @@@Table.DisplayName@@@
{
    /// <summary>
    /// Command to create a new @@@Table.DisplayName@@@
    /// </summary>
    public class Create@@@Table.DisplayName @@@Command : IRequest<Result<long>>
    {
<FOREACH>
    <LOOP>TableGenerate.ColumnsNotKey</LOOP>
    <CONTENT>        public ###SourceCodeType### ###ColumnName### { get; set; }</CONTENT>
</FOREACH>
    }
}