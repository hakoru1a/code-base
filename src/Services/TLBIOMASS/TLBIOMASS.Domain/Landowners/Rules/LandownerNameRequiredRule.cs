using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Landowners.Rules;

public class LandownerNameRequiredRule : IBusinessRule
{
    private readonly string _name;

    public LandownerNameRequiredRule(string name) => _name = name;

    public bool IsBroken() => string.IsNullOrWhiteSpace(_name);
    public string Message => "Landowner name is required";
    public string Code => "Landowner.NameRequired";
}
