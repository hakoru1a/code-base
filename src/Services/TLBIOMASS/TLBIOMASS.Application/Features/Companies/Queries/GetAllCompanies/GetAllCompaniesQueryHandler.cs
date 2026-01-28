using MediatR;
using TLBIOMASS.Domain.Companies.Interfaces;
using Shared.DTOs.Company;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TLBIOMASS.Domain.Companies.Specifications;
using System.Collections.Generic;
using System.Linq;

namespace TLBIOMASS.Application.Features.Companies.Queries.GetAllCompanies;

public class GetAllCompaniesQueryHandler : IRequestHandler<GetAllCompaniesQuery, List<CompanyResponseDto>>
{
    private readonly ICompanyRepository _repository;

    public GetAllCompaniesQueryHandler(ICompanyRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CompanyResponseDto>> Handle(GetAllCompaniesQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll();

        // 1. Apply Search Filter
        if (!string.IsNullOrEmpty(request.Filter.SearchTerms))
        {
            var spec = new CompanySearchSpecification(request.Filter.SearchTerms);
            query = query.Where(spec.ToExpression());
        }

        // 2. Fetch and Adapt (No dynamic sorting)
        var items = await query.ToListAsync(cancellationToken);
        return items.Adapt<List<CompanyResponseDto>>();
    }
}
