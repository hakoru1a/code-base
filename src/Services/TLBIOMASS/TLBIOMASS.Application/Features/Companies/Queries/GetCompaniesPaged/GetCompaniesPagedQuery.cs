using MediatR;
using Shared.SeedWork;
using Shared.DTOs.Company;

namespace TLBIOMASS.Application.Features.Companies.Queries.GetCompaniesPaged;

public class GetCompaniesPagedQuery : IRequest<PagedList<CompanyResponseDto>>
{
    public CompanyPagedFilterDto Filter { get; set; } = new();
}

