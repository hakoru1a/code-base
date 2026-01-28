using Contracts.Domain.Interface;
using System.Linq.Expressions;
using TLBIOMASS.Domain.WeighingTickets;

namespace TLBIOMASS.Domain.WeighingTickets.Specifications;

public class WeighingTicketSearchSpecification : ISpecification<WeighingTicket>
{
    private readonly string _searchTerm;

    public WeighingTicketSearchSpecification(string searchTerm)
    {
        _searchTerm = !string.IsNullOrWhiteSpace(searchTerm) 
            ? searchTerm.ToLowerInvariant() 
            : string.Empty;
    }

    public bool IsSatisfiedBy(WeighingTicket ticket)
    {
        return ToExpression().Compile()(ticket);
    }

    public Expression<Func<WeighingTicket, bool>> ToExpression()
    {
        if (string.IsNullOrEmpty(_searchTerm))
            return c => true;
            
        return x => x.TicketNumber.ToLower().Contains(_searchTerm) || 
                    x.VehiclePlate.ToLower().Contains(_searchTerm) ||
                    x.CustomerName.ToLower().Contains(_searchTerm) ||
                    x.MaterialName.ToLower().Contains(_searchTerm);
    }
}
