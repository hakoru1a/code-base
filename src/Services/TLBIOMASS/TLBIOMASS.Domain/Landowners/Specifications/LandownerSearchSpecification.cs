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
                   (c.Phone != null && c.Phone.Contains(_searchTerm)) ||
                   (c.Email != null && c.Email.ToLower().Contains(_searchTerm)) ||
                   (c.BankAccount != null && c.BankAccount.Contains(_searchTerm)) ||
                   (c.IdentityCardNo != null && c.IdentityCardNo.Contains(_searchTerm)) ||
                   (c.BankName != null && c.BankName.ToLower().Contains(_searchTerm));
    }
}
