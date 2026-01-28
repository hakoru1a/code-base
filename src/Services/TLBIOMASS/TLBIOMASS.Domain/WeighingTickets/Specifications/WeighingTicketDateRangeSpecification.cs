using Contracts.Domain.Interface;
using System.Linq.Expressions;
using TLBIOMASS.Domain.WeighingTickets;

namespace TLBIOMASS.Domain.WeighingTickets.Specifications;

public class WeighingTicketDateRangeSpecification : ISpecification<WeighingTicket>
{
    private readonly DateTime? _fromDate;
    private readonly DateTime? _toDate;

    public WeighingTicketDateRangeSpecification(DateTime? fromDate, DateTime? toDate)
    {
        _fromDate = fromDate;
        _toDate = toDate;
    }

    public bool IsSatisfiedBy(WeighingTicket ticket)
    {
        return ToExpression().Compile()(ticket);
    }

    public Expression<Func<WeighingTicket, bool>> ToExpression()
    {
        return x => (!_fromDate.HasValue || x.CreatedDate.Date >= _fromDate.Value.Date) &&
                    (!_toDate.HasValue || x.CreatedDate.Date <= _toDate.Value.Date);
    }
}
