using MediatR;
using Shared.DTOs.Company;

namespace TLBIOMASS.Application.Features.Companies.Queries.GetCompanies;

public class GetCompaniesQuery : IRequest<List<CompanyResponseDto>>
{
    public CompanyFilterDto Filter { get; set; } = new();
}

