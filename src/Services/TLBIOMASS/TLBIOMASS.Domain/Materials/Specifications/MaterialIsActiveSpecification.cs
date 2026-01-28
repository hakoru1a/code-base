using System.Linq.Expressions;
using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Materials.Specifications;

public class MaterialIsActiveSpecification : ISpecification<Material>
{
    private readonly bool _isActive;

    public MaterialIsActiveSpecification(bool isActive) => _isActive = isActive;

    public bool IsSatisfiedBy(Material material) => material.IsActive == _isActive;

    public Expression<Func<Material, bool>> ToExpression()
    {
        return c => c.IsActive == _isActive;
    }
}
