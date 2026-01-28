using Contracts.Domain.Interface;
using System.Linq.Expressions;
using TLBIOMASS.Domain.WeighingTickets;

namespace TLBIOMASS.Domain.WeighingTickets.Specifications;

public class WeighingTicketIsCompletedSpecification : ISpecification<WeighingTicket>
{
    private readonly bool _expectCompleted;

    public WeighingTicketIsCompletedSpecification(bool expectCompleted)
    {
        _expectCompleted = expectCompleted;
    }

    public bool IsSatisfiedBy(WeighingTicket ticket)
    {
        return ToExpression().Compile()(ticket);
    }

    public Expression<Func<WeighingTicket, bool>> ToExpression()
    {
        if (_expectCompleted)
            return x => x.SecondWeighingTime != null;
        
        return x => x.SecondWeighingTime == null;
    }
}
