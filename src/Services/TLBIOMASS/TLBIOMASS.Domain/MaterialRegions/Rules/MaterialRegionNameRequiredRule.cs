using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.MaterialRegions.Rules;

public class MaterialRegionNameRequiredRule : IBusinessRule
{
    private readonly string _name;

    public MaterialRegionNameRequiredRule(string name) => _name = name;

    public bool IsBroken() => string.IsNullOrWhiteSpace(_name);
    public string Message => "Material region name is required";
    public string Code => "MaterialRegion.NameRequired";
}
