using MediatR;
using TLBIOMASS.Domain.Companies.Interfaces;

namespace TLBIOMASS.Application.Features.Companies.Commands.DeleteCompany;

public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, bool>
{
    private readonly ICompanyRepository _repository;

    public DeleteCompanyCommandHandler(ICompanyRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _repository.GetByIdAsync(request.Id);
        if (company == null) return false;

        await _repository.DeleteAsync(company);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
