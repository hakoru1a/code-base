using MediatR;
using TLBIOMASS.Domain.Customers.Events;
using TLBIOMASS.Domain.Customers;
using TLBIOMASS.Domain.Customers.Interfaces;

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
            request.Phone,
            request.Address,
            request.Email,
            request.TaxCode,
            request.Note
        );

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
