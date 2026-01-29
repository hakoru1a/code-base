using MediatR;
using TLBIOMASS.Domain.Agencies.Interfaces;
using Shared.DTOs.Agency;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace TLBIOMASS.Application.Features.Agencies.Queries.GetAgencies;

public class GetAgenciesQueryHandler : IRequestHandler<GetAgenciesQuery, List<AgencyResponseDto>>
{
    private readonly IAgencyRepository _repository;

    public GetAgenciesQueryHandler(IAgencyRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AgencyResponseDto>> Handle(GetAgenciesQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll(false, x => x.BankAccounts.Where(b => b.OwnerType == "Agency"));

        // 1. Apply Search Filter
        if (!string.IsNullOrEmpty(request.Filter.SearchTerms))
        {
            var search = request.Filter.SearchTerms.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search) ||
                               (c.Contact != null && c.Contact.Phone != null && c.Contact.Phone.Contains(search)) ||
                               (c.Contact != null && c.Contact.Email != null && c.Contact.Email.ToLower().Contains(search)) ||
                               (c.Contact != null && c.Contact.Address != null && c.Contact.Address.ToLower().Contains(search)) ||
                               (c.BankAccounts.Any(ba => ba.AccountNumber.Contains(search) || ba.BankName.ToLower().Contains(search))) ||
                               (c.Identity != null && c.Identity.IdentityNumber != null && c.Identity.IdentityNumber.Contains(search)));
        }

        // 2. Fetch and Adapt
        var entities = await query.ToListAsync(cancellationToken);
        return entities.Adapt<List<AgencyResponseDto>>();
    }
}

