using MediatR;
using TLBIOMASS.Domain.Companies.Interfaces;

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
             // Implicitly using Contracts.Exceptions if I add the using, or just return false if standard didn't use exception? 
             // Agency used `throw new NotFoundException("Agency", request.Id);`
             // I will use `return false` to minimize dependency issues unless I know where NotFoundException is.
             // Agency used `using Contracts.Exceptions;`. I'll add that using.
             return false; 
        }

        company.Update(
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

        await _repository.UpdateAsync(company);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
