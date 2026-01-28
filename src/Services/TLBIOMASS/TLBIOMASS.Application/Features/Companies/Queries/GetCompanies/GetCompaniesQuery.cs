using MediatR;
using Shared.SeedWork;
using Shared.DTOs.Company;

namespace TLBIOMASS.Application.Features.Companies.Queries.GetCompanies;

public class GetCompaniesQuery : IRequest<PagedList<CompanyResponseDto>>
{
    public CompanyPagedFilterDto Filter { get; set; } = new();
}
