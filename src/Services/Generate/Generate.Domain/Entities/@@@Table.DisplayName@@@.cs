@@@copyRight@@@
using Generate.Domain;

namespace Generate.Domain.Entities
{
    /// <summary>
    /// @@@Table.DisplayName@@@ entity
    /// </summary>
    public class @@@Table.DisplayName@@@ : EntityAuditBase<long>
    {
<FOREACH>
    <LOOP>TableGenerate.NotGenerateColumn1</LOOP>
    <CONDITIONS>
        <CONDITION>
            <CONDITIONNAME>###IsForeignKeyColumn###</CONDITIONNAME>
            <CONDITIONOPERATOR>=</CONDITIONOPERATOR>
            <CONDITIONVALUE>False</CONDITIONVALUE>
            <CONTENT>        
        /// <summary>
        /// ###Description###
        /// </summary>
        public ###SourceCodeType### ###ColumnName### { get; set; }</CONTENT>
        </CONDITION>
    </CONDITIONS>
</FOREACH>

<FOREACH>
    <LOOP>Tables.ForeignTable</LOOP>       
    <CONTENT>@@@TableForeign.displayName@@@;</CONTENT>
</FOREACH>  


<FOREACH>
    <LOOP>TableGenerate.ForeignTableList</LOOP> 
    <CONTENT>###DisplayName###, </CONTENT>
</FOREACH>

<FOREACH>
    <LOOP>Tables.ForeignKeyBy</LOOP>
    <CONTENT>
        public virtual ICollection<@@@TableForeign.DisplayName@@@> @@@TableForeign.DisplayName@@@s { get; set; } = new List<@@@TableForeign.DisplayName@@@>();</CONTENT>
</FOREACH>

    }
}