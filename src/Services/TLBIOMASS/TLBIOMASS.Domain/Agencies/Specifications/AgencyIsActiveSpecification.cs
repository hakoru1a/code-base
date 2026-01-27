using System.Linq.Expressions;
using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.Agencies.Specifications;

public class AgencyIsActiveSpecification : ISpecification<Agency>
{
    private readonly bool _isActive;

    public AgencyIsActiveSpecification(bool isActive) => _isActive = isActive;

    public bool IsSatisfiedBy(Agency agency) => agency.IsActive == _isActive;

    public Expression<Func<Agency, bool>> ToExpression()
    {
        return c => c.IsActive == _isActive;
    }
}
