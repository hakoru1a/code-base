using Contracts.Domain.Interface;
using System.Linq.Expressions;

namespace TLBIOMASS.Domain.Payments.Specifications;

public class PaymentDetailAgencySpecification : ISpecification<PaymentDetail>
{
    private readonly int _agencyId;

    public PaymentDetailAgencySpecification(int agencyId)
    {
        _agencyId = agencyId;
    }

    public bool IsSatisfiedBy(PaymentDetail entity)
    {
        return ToExpression().Compile()(entity);
    }

    public Expression<Func<PaymentDetail, bool>> ToExpression()
    {
        return x => x.AgencyId == _agencyId;
    }
}
