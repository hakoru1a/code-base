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

        return c => c.Detail.RegionName.ToLower().Contains(_searchTerm) ||
                   (c.Detail.Address != null && c.Detail.Address.ToLower().Contains(_searchTerm)) ||
                   (c.Detail.CertificateId != null && c.Detail.CertificateId.Contains(_searchTerm));
    }
}
