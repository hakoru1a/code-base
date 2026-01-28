using MediatR;
using TLBIOMASS.Domain.Companies.Interfaces;
using Shared.DTOs.Company;
using Shared.SeedWork;
using Mapster;
using System.Linq;
using TLBIOMASS.Domain.Companies;

namespace TLBIOMASS.Application.Features.Companies.Queries.GetCompaniesPaged;

public class GetCompaniesPagedQueryHandler : IRequestHandler<GetCompaniesPagedQuery, PagedList<CompanyResponseDto>>
{
    private readonly ICompanyRepository _repository;

    public GetCompaniesPagedQueryHandler(ICompanyRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<CompanyResponseDto>> Handle(GetCompaniesPagedQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll();

        if (!string.IsNullOrEmpty(request.Filter.SearchTerms))
        {
            var search = request.Filter.SearchTerms.Trim().ToLower();
            query = query.Where(c => c.CompanyName.ToLower().Contains(search) ||
                               (c.Contact != null && c.Contact.Phone != null && c.Contact.Phone.Contains(search)) ||
                               (c.Contact != null && c.Contact.Email != null && c.Contact.Email.ToLower().Contains(search)) ||
                               (c.Contact != null && c.Contact.Address != null && c.Contact.Address.ToLower().Contains(search)) ||
                               (c.TaxCode != null && c.TaxCode.ToLower().Contains(search)) ||
                               (c.Representative != null && c.Representative.Name != null && c.Representative.Name.ToLower().Contains(search)));
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
            "email" => isDescending ? query.OrderByDescending(x => x.Contact != null ? x.Contact.Email : null) : query.OrderBy(x => x.Contact != null ? x.Contact.Email : null),
            "createddate" or "created" => isDescending ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
            "lastmodifieddate" or "updated" => isDescending ? query.OrderByDescending(x => x.LastModifiedDate) : query.OrderBy(x => x.LastModifiedDate),
            _ => query.OrderByDescending(x => x.CreatedDate)
        };
    }
}

