using MediatR;
using TLBIOMASS.Domain.Companies.Interfaces;
using Shared.DTOs.Company;
using Mapster;

namespace TLBIOMASS.Application.Features.Companies.Queries.GetCompanyById;

public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyResponseDto>
{
    private readonly ICompanyRepository _repository;

    public GetCompanyByIdQueryHandler(ICompanyRepository repository)
    {
        _repository = repository;
    }

    public async Task<CompanyResponseDto> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        var company = await _repository.GetByIdAsync(request.Id);
        return company.Adapt<CompanyResponseDto>();
    }
}
