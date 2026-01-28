using System.Linq.Expressions;
using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Agencies.Specifications;

public class AgencySearchSpecification : ISpecification<Agency>
{
    private readonly string _searchTerm;

    public AgencySearchSpecification(string searchTerm)
    {
        _searchTerm = !string.IsNullOrWhiteSpace(searchTerm) 
            ? searchTerm.ToLowerInvariant() 
            : string.Empty;
    }

    public bool IsSatisfiedBy(Agency agency)
    {
        return ToExpression().Compile()(agency);
    }

    public Expression<Func<Agency, bool>> ToExpression()
    {
        if (string.IsNullOrEmpty(_searchTerm))
            return c => true;

        return c => c.Name.ToLower().Contains(_searchTerm) ||
                   (c.Contact != null && c.Contact.Phone != null && c.Contact.Phone.Contains(_searchTerm)) ||
                   (c.Contact != null && c.Contact.Email != null && c.Contact.Email.ToLower().Contains(_searchTerm)) ||
                   (c.Contact != null && c.Contact.Address != null && c.Contact.Address.ToLower().Contains(_searchTerm)) ||
                   (c.Bank != null && c.Bank.BankAccount != null && c.Bank.BankAccount.Contains(_searchTerm)) ||
                   (c.Bank != null && c.Bank.BankName != null && c.Bank.BankName.ToLower().Contains(_searchTerm)) ||
                   (c.Identity != null && c.Identity.IdentityNumber != null && c.Identity.IdentityNumber.Contains(_searchTerm));
    }
}
