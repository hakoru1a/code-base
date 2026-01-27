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
                   (c.Phone != null && c.Phone.Contains(_searchTerm)) ||
                   (c.BankAccount != null && c.BankAccount.Contains(_searchTerm)) ||
                   (c.IdentityNumber != null && c.IdentityNumber.Contains(_searchTerm)) ||
                   (c.BankName != null && c.BankName.ToLower().Contains(_searchTerm));
    }
}
