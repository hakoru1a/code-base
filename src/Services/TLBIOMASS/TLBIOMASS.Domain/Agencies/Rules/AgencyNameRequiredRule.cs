using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Agencies.Rules;

public class AgencyNameRequiredRule : IBusinessRule
{
    private readonly string _name;

    public AgencyNameRequiredRule(string name) => _name = name;

    public bool IsBroken() => string.IsNullOrWhiteSpace(_name);
    public string Message => "Agency name is required";
    public string Code => "Agency.NameRequired";
}
