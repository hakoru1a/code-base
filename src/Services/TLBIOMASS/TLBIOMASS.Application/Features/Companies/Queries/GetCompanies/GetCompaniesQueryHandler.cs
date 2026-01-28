using MediatR;
using TLBIOMASS.Domain.Companies.Interfaces;
using Shared.DTOs.Company;
using Shared.SeedWork;
using Mapster;
using System.Linq.Expressions;
using TLBIOMASS.Domain.Companies;
using TLBIOMASS.Domain.Companies.Specifications;

namespace TLBIOMASS.Application.Features.Companies.Queries.GetCompanies;

public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, PagedList<CompanyResponseDto>>
{
    private readonly ICompanyRepository _repository;

    public GetCompaniesQueryHandler(ICompanyRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<CompanyResponseDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll();

        if (!string.IsNullOrEmpty(request.Filter.SearchTerms))
        {
            var spec = new CompanySearchSpecification(request.Filter.SearchTerms);
            query = query.Where(spec.ToExpression());
        }

        // Apply sorting
        query = ApplySorting(query, request.Filter.OrderBy, request.Filter.OrderByDirection);

        var pagedItems = await _repository.GetPageAsync(query, request.Filter.PageNumber, request.Filter.PageSize, cancellationToken);

        return new PagedList<CompanyResponseDto>(
            pagedItems.Adapt<List<CompanyResponseDto>>(),
            pagedItems.GetMetaData().TotalItems,
            request.Filter.PageNumber, request.Filter.PageSize);
    }

    private IQueryable<Company> ApplySorting(IQueryable<Company> query, string? sortBy, string? sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderByDescending(x => x.CreatedDate);
        }

        return sortBy.ToLower() switch
        {
            "companyname" => isDescending ? query.OrderByDescending(x => x.CompanyName) : query.OrderBy(x => x.CompanyName),
            "email" => isDescending ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
            "createddate" or "created" => isDescending ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
            "lastmodifieddate" or "updated" => isDescending ? query.OrderByDescending(x => x.LastModifiedDate) : query.OrderBy(x => x.LastModifiedDate),
            _ => query.OrderByDescending(x => x.CreatedDate)
        };
    }
}
