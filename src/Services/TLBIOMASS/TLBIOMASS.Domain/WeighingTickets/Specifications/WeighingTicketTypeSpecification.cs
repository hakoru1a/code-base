using Contracts.Domain.Interface;
using System.Linq.Expressions;
using TLBIOMASS.Domain.WeighingTickets;

namespace TLBIOMASS.Domain.WeighingTickets.Specifications;

public class WeighingTicketTypeSpecification : ISpecification<WeighingTicket>
{
    private readonly string _ticketType;

    public WeighingTicketTypeSpecification(string ticketType)
    {
        _ticketType = !string.IsNullOrWhiteSpace(ticketType) 
            ? ticketType.ToLowerInvariant() 
            : string.Empty;
    }

    public bool IsSatisfiedBy(WeighingTicket ticket)
    {
        return ToExpression().Compile()(ticket);
    }

    public Expression<Func<WeighingTicket, bool>> ToExpression()
    {
        if (string.IsNullOrEmpty(_ticketType))
            return c => true;
            
        return x => x.TicketType.ToLower().Contains(_ticketType);
    }
}
