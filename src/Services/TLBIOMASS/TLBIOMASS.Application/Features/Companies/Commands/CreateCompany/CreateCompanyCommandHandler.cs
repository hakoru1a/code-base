using MediatR;
using TLBIOMASS.Domain.Companies;
using TLBIOMASS.Domain.Companies.Interfaces;

namespace TLBIOMASS.Application.Features.Companies.Commands.CreateCompany;

public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, long>
{
    private readonly ICompanyRepository _repository;

    public CreateCompanyCommandHandler(ICompanyRepository repository)
    {
        _repository = repository;
    }

    public async Task<long> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = Company.Create(
            request.CompanyName,
            request.Address,
            request.TaxCode,
            request.Representative,
            request.Position,
            request.PhoneNumber,
            request.Email,
            request.IdentityCardNo,
            request.IssuePlace,
            request.IssueDate);

        //company.AddDomainEvent(new CompanyCreatedEvent(company.Id, company.CompanyName));

        await _repository.CreateAsync(company);
        await _repository.SaveChangesAsync(cancellationToken);

        return (long)company.Id;
    }
}
