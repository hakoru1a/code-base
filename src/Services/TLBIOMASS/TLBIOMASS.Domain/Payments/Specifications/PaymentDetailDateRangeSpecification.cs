using Contracts.Domain.Interface;
using System.Linq.Expressions;

namespace TLBIOMASS.Domain.Payments.Specifications;

public class PaymentDetailDateRangeSpecification : ISpecification<PaymentDetail>
{
    private readonly DateTime? _from;
    private readonly DateTime? _to;

    public PaymentDetailDateRangeSpecification(DateTime? from, DateTime? to)
    {
        _from = from;
        _to = to;
    }

    public bool IsSatisfiedBy(PaymentDetail entity)
    {
        return ToExpression().Compile()(entity);
    }

    public Expression<Func<PaymentDetail, bool>> ToExpression()
    {
        return x => (!_from.HasValue || x.PaymentDate.Date >= _from.Value.Date) &&
                    (!_to.HasValue || x.PaymentDate.Date <= _to.Value.Date);
    }
}
