using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Materials.Rules;

public class MaterialNameRequiredRule : IBusinessRule
{
    private readonly string _name;

    public MaterialNameRequiredRule(string name) => _name = name;

    public bool IsBroken() => string.IsNullOrWhiteSpace(_name);
    public string Message => "Material name is required";
    public string Code => "Material.NameRequired";
}
