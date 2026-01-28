using System.Linq.Expressions;
using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Materials.Specifications;

public class MaterialSearchSpecification : ISpecification<Material>
{
    private readonly string _searchTerm;

    public MaterialSearchSpecification(string searchTerm)
    {
        _searchTerm = !string.IsNullOrWhiteSpace(searchTerm) 
            ? searchTerm.ToLowerInvariant() 
            : string.Empty;
    }

    public bool IsSatisfiedBy(Material material)
    {
        return ToExpression().Compile()(material);
    }

    public Expression<Func<Material, bool>> ToExpression()
    {
        if (string.IsNullOrEmpty(_searchTerm))
            return c => true;

        return c => c.Spec.Name.ToLower().Contains(_searchTerm) ||
                   c.Spec.Unit.ToLower().Contains(_searchTerm) ||
                   (c.Spec.Description != null && c.Spec.Description.ToLower().Contains(_searchTerm));
    }
}
