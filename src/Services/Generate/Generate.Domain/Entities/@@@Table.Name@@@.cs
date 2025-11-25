@@@copyRight@@@
using Generate.Domain.Common;

namespace Generate.Domain.Entities
{
    /// <summary>
    /// @@@Table.Name@@@ entity
    /// </summary>
    public class @@@Table.Name@@@ : BaseEntity
    {
<FOREACH>
    <LOOP>TableGenerate.ColumnsNotKey</LOOP>
    <CONTENT>        /// <summary>
        /// ###Description###
        /// </summary>
        public ###ConvertTypeDbToCode### ###Name### { get; set; }</CONTENT>
</FOREACH>

        // Navigation properties
<FOREACH>
    <LOOP>Tables.ForeignTable</LOOP>
    <CONTENT>        public virtual @@@TableForeign.Name@@@? @@@TableForeign.Name@@@ { get; set; }</CONTENT>
</FOREACH>

<FOREACH>
    <LOOP>Tables.ForeignKeyBy</LOOP>
    <CONTENT>        public virtual ICollection<@@@TableForeignBy.Name@@@> @@@TableForeignBy.Name@@@s { get; set; } = new List<@@@TableForeignBy.Name@@@>();</CONTENT>
</FOREACH>
    }
}