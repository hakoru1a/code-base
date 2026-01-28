using System.Linq.Expressions;
using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.MaterialRegions.Specifications;

public class MaterialRegionSearchSpecification : ISpecification<MaterialRegion>
{
    private readonly string _searchTerm;

    public MaterialRegionSearchSpecification(string searchTerm)
    {
        _searchTerm = !string.IsNullOrWhiteSpace(searchTerm) 
            ? searchTerm.ToLowerInvariant() 
            : string.Empty;
    }

    public bool IsSatisfiedBy(MaterialRegion region)
    {
        return ToExpression().Compile()(region);
    }

    public Expression<Func<MaterialRegion, bool>> ToExpression()
    {
        if (string.IsNullOrEmpty(_searchTerm))
            return c => true;

        return c => c.RegionName.ToLower().Contains(_searchTerm) ||
                   (c.Address != null && c.Address.ToLower().Contains(_searchTerm)) ||
                   (c.CertificateID != null && c.CertificateID.Contains(_searchTerm));
    }
}
