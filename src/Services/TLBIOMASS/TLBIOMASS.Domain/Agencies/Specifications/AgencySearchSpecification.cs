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
                   (c.Phone != null && c.Phone.Contains(_searchTerm)) ||
                   (c.Email != null && c.Email.ToLower().Contains(_searchTerm)) ||
                   (c.BankAccount != null && c.BankAccount.Contains(_searchTerm)) ||
                   (c.IdentityCard != null && c.IdentityCard.Contains(_searchTerm)) ||
                   (c.BankName != null && c.BankName.ToLower().Contains(_searchTerm));
    }
}
