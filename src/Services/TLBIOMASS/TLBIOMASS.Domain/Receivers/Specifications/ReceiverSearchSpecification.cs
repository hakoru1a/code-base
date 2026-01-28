using System.Linq.Expressions;
using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Receivers.Specifications;

public class ReceiverSearchSpecification : ISpecification<Receiver>
{
    private readonly string _searchTerm;

    public ReceiverSearchSpecification(string searchTerm)
    {
        _searchTerm = !string.IsNullOrWhiteSpace(searchTerm) 
            ? searchTerm.ToLowerInvariant() 
            : string.Empty;
    }

    public bool IsSatisfiedBy(Receiver receiver)
    {
        return ToExpression().Compile()(receiver);
    }

    public Expression<Func<Receiver, bool>> ToExpression()
    {
        if (string.IsNullOrEmpty(_searchTerm))
            return c => true;

        return c => c.Name.ToLower().Contains(_searchTerm) ||
                   (c.Contact != null && c.Contact.Phone != null && c.Contact.Phone.Contains(_searchTerm)) ||
                   (c.Contact != null && c.Contact.Address != null && c.Contact.Address.ToLower().Contains(_searchTerm)) ||
                   (c.Bank != null && c.Bank.BankAccount != null && c.Bank.BankAccount.Contains(_searchTerm)) ||
                   (c.Bank != null && c.Bank.BankName != null && c.Bank.BankName.ToLower().Contains(_searchTerm)) ||
                   (c.Identity != null && c.Identity.IdentityNumber != null && c.Identity.IdentityNumber.Contains(_searchTerm));
    }
}
