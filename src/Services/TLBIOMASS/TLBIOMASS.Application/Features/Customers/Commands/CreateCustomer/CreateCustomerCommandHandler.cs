using MediatR;
using TLBIOMASS.Domain.Customers.Events;
using TLBIOMASS.Domain.Customers;
using TLBIOMASS.Domain.Customers.Interfaces;
using Shared.Domain.ValueObjects;

namespace TLBIOMASS.Application.Features.Customers.Commands.CreateCustomer;


public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, int>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMediator _mediator;

    public CreateCustomerCommandHandler(ICustomerRepository customerRepository, IMediator mediator)
    {
        _customerRepository = customerRepository;
        _mediator = mediator;
    }

    public async Task<int> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = Customer.Create(
            request.Name,
            new ContactInfo(request.Phone, request.Email, request.Address, request.Note),
            request.TaxCode);

        // Check tax code uniqueness via Domain Rule
        customer.CheckTaxCodeUnique(_customerRepository);

        var result = await _customerRepository.CreateAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        // Publish event
        await _mediator.Publish(new CustomerCreatedEvent
        {
            CustomerId = result,
            Name = customer.Name
        }, cancellationToken);

        return result;
    }
}
