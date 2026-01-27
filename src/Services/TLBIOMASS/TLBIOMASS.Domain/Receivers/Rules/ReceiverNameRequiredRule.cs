using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Receivers.Rules;

public class ReceiverNameRequiredRule : IBusinessRule
{
    private readonly string _name;

    public ReceiverNameRequiredRule(string name) => _name = name;

    public bool IsBroken() => string.IsNullOrWhiteSpace(_name);
    public string Message => "Receiver name is required";
    public string Code => "Receiver.NameRequired";
}
