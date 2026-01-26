using MediatR;
using TLBIOMASS.Domain.Customers.Events;
using TLBIOMASS.Domain.Customers;
using TLBIOMASS.Domain.Customers.Interfaces;

namespace TLBIOMASS.Application.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, bool>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMediator _mediator;

    public UpdateCustomerCommandHandler(ICustomerRepository customerRepository, IMediator mediator)
    {
        _customerRepository = customerRepository;
        _mediator = mediator;
    }

    public async Task<bool> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        // Get customer from database
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
        {
            return false;
        }

        // Use Domain method to update
        customer.Update(
            request.Name,
            request.Phone,
            request.Address,
            request.Email,
            request.TaxCode,
            request.Note
        );

        // Check tax code uniqueness via Domain Rule
        customer.CheckTaxCodeUnique(_customerRepository);

        // Update Active status
        if (request.IsActive)
            customer.Activate();
        else
            customer.Deactivate();

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        // Publish event
        await _mediator.Publish(new CustomerUpdatedEvent
        {
            CustomerId = customer.Id,
            Name = customer.Name
        }, cancellationToken);

        return true;
    }
}
