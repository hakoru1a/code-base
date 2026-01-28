using System.Linq.Expressions;
using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Landowners.Specifications;

public class LandownerSearchSpecification : ISpecification<Landowner>
{
    private readonly string _searchTerm;

    public LandownerSearchSpecification(string searchTerm)
    {
        _searchTerm = !string.IsNullOrWhiteSpace(searchTerm) 
            ? searchTerm.ToLowerInvariant() 
            : string.Empty;
    }

    public bool IsSatisfiedBy(Landowner landowner)
    {
        return ToExpression().Compile()(landowner);
    }

    public Expression<Func<Landowner, bool>> ToExpression()
    {
        if (string.IsNullOrEmpty(_searchTerm))
            return c => true;

        return c => c.Name.ToLower().Contains(_searchTerm) ||
                   (c.Contact != null && c.Contact.Phone != null && c.Contact.Phone.Contains(_searchTerm)) ||
                   (c.Contact != null && c.Contact.Email != null && c.Contact.Email.ToLower().Contains(_searchTerm)) ||
                   (c.Bank != null && c.Bank.BankAccount != null && c.Bank.BankAccount.Contains(_searchTerm)) ||
                   (c.Identity != null && c.Identity.IdentityNumber != null && c.Identity.IdentityNumber.Contains(_searchTerm)) ||
                   (c.Bank != null && c.Bank.BankName != null && c.Bank.BankName.ToLower().Contains(_searchTerm));
    }
}
