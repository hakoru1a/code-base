using MediatR;
using TLBIOMASS.Domain.Companies.Interfaces;
using Shared.Domain.ValueObjects;
using TLBIOMASS.Domain.Companies.ValueObjects;

namespace TLBIOMASS.Application.Features.Companies.Commands.UpdateCompany;

public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, bool>
{
    private readonly ICompanyRepository _repository;

    public UpdateCompanyCommandHandler(ICompanyRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _repository.GetByIdAsync(request.Id);
        if (company == null)
        {
             return false; 
        }

        company.Update(
            request.CompanyName,
            request.TaxCode,
            new RepresentativeInfo(request.Representative, request.Position),
            new ContactInfo(request.PhoneNumber, request.Email, request.Address, null),
            new IdentityInfo(request.IdentityCardNo, request.IssuePlace, request.IssueDate, null));

        await _repository.UpdateAsync(company);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
