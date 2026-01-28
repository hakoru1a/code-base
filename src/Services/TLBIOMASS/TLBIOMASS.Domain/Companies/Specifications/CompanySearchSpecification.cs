using Contracts.Domain.Interface;
using System.Linq.Expressions;

namespace TLBIOMASS.Domain.Companies.Specifications;

public class CompanySearchSpecification : ISpecification<Company>
{
    private readonly string _searchTerm;

    public CompanySearchSpecification(string searchTerm)
    {
        _searchTerm = !string.IsNullOrWhiteSpace(searchTerm) 
            ? searchTerm.ToLowerInvariant() 
            : string.Empty;
    }

    public bool IsSatisfiedBy(Company company)
    {
        return ToExpression().Compile()(company);
    }

    public Expression<Func<Company, bool>> ToExpression()
    {
        if (string.IsNullOrEmpty(_searchTerm))
            return c => true;
            
        return x => x.CompanyName.ToLower().Contains(_searchTerm) || 
                    (x.TaxCode != null && x.TaxCode.Contains(_searchTerm)) ||
                    (x.Representative != null && x.Representative.ToLower().Contains(_searchTerm));
    }
}
