using Generate.Domain;

namespace Generate.Domain.Entities
{
    public class @@@Table.DisplayName@@@ : EntityAuditBase<long>
    {
<FOREACH>
    <LOOP>TableGenerate.NotGenerateColumn1</LOOP>
    <CONDITIONS>
        <CONDITION>
            <CONDITIONNAME>###IsForeignKeyColumn###</CONDITIONNAME>
            <CONDITIONOPERATOR>=</CONDITIONOPERATOR>
            <CONDITIONVALUE>True</CONDITIONVALUE>
            <CONTENT>
        public long ###ColumnName### { get; set; }</CONTENT>
        </CONDITION>
        <RELATIONSHIP>Else</RELATIONSHIP>
            <CONTENT>
        public ###SourceCodeType### ###ColumnName### { get; set; }</CONTENT>
    </CONDITIONS>
</FOREACH>
<FOREACH>
    <LOOP>Tables.ForeignTable</LOOP> 
    <CONTENT>
        public virtual @@@Table.DisplayName@@@ @@@Table.DisplayName@@@ { get; set; }
    </CONTENT>
</FOREACH>
<FOREACH>
    <LOOP>Tables.ForeignKeyBy</LOOP>
    <CONTENT>
        public virtual ICollection<@@@Table.DisplayName@@@> @@@Table.DisplayName@@@s { get; set; } = new List<@@@Table.DisplayName@@@>();</CONTENT>
</FOREACH>
    }
}       