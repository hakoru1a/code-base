using MediatR;

namespace TLBIOMASS.Application.Features.Customers.Commands.DeleteCustomer;

public class DeleteCustomerCommand : IRequest<bool>
{
    public int Id { get; set; }
}
