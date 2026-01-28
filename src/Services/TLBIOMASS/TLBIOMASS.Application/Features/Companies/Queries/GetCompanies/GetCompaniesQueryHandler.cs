using MediatR;
using TLBIOMASS.Domain.Companies.Interfaces;
using Shared.DTOs.Company;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace TLBIOMASS.Application.Features.Companies.Queries.GetCompanies;

public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, List<CompanyResponseDto>>
{
    private readonly ICompanyRepository _repository;

    public GetCompaniesQueryHandler(ICompanyRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CompanyResponseDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll();

        // 1. Apply Search Filter
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

        // 2. Fetch and Adapt (No dynamic sorting)
        var items = await query.ToListAsync(cancellationToken);
        return items.Adapt<List<CompanyResponseDto>>();
    }
}

