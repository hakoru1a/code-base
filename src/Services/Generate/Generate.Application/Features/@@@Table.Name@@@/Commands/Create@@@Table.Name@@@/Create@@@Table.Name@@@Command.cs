@@@copyRight@@@
using MediatR;
using Shared.SeedWork;

namespace Generate.Application.Features.@@@Table.Name@@@.Commands.Create@@@Table.Name@@@
{
    /// <summary>
    /// Command to create a new @@@Table.Name@@@
    /// </summary>
    public class Create@@@Table.Name@@@Command : IRequest<Result<long>>
    {
<FOREACH>
    <LOOP>TableGenerate.ColumnsNotKey</LOOP>
    <CONTENT>        public ###ConvertTypeDbToCode### ###Name### { get; set; }</CONTENT>
</FOREACH>
    }
}