using MediatR;
using Shared.DTOs.Company;

namespace TLBIOMASS.Application.Features.Companies.Queries.GetAllCompanies;

public class GetAllCompaniesQuery : IRequest<List<CompanyResponseDto>>
{
    public CompanyFilterDto Filter { get; set; } = new();
}
