using System.Linq.Expressions;
using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Landowners.Specifications;

public class LandownerIsActiveSpecification : ISpecification<Landowner>
{
    private readonly bool _isActive;

    public LandownerIsActiveSpecification(bool isActive)
    {
        _isActive = isActive;
    }

    public bool IsSatisfiedBy(Landowner landowner)
    {
        return landowner.IsActive == _isActive;
    }

    public Expression<Func<Landowner, bool>> ToExpression()
    {
        return c => c.IsActive == _isActive;
    }
}
