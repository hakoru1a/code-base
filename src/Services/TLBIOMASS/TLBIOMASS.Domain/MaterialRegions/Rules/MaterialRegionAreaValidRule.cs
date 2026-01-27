using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.MaterialRegions.Rules;

public class MaterialRegionAreaValidRule : IBusinessRule
{
    private readonly decimal _areaHa;

    public MaterialRegionAreaValidRule(decimal areaHa) => _areaHa = areaHa;

    public bool IsBroken() => _areaHa < 0;
    public string Message => "Area cannot be negative";
    public string Code => "MaterialRegion.AreaInvalid";
}
