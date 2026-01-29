using MediatR;
using Shared.DTOs.Receiver;
using TLBIOMASS.Domain.Receivers.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Shared.Domain.Enums;

namespace TLBIOMASS.Application.Features.Receivers.Queries.GetReceivers;

public class GetReceiversQueryHandler : IRequestHandler<GetReceiversQuery, List<ReceiverResponseDto>>
{
    private readonly IReceiverRepository _repository;

    public GetReceiversQueryHandler(IReceiverRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ReceiverResponseDto>> Handle(GetReceiversQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll(false, x => x.BankAccounts.Where(b => b.OwnerType == OwnerType.Receiver));

        // 1. Apply Search Filter
        if (!string.IsNullOrEmpty(request.Filter.SearchTerms))
        {
            var search = request.Filter.SearchTerms.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search) ||
                               (c.Contact != null && c.Contact.Phone != null && c.Contact.Phone.Contains(search)) ||
                               (c.Contact != null && c.Contact.Address != null && c.Contact.Address.ToLower().Contains(search)) ||
                               (c.BankAccounts.Any(ba => ba.AccountNumber.Contains(search) || ba.BankName.ToLower().Contains(search))) ||
                               (c.Identity != null && c.Identity.IdentityNumber != null && c.Identity.IdentityNumber.Contains(search)));
        }

        var items = await query.ToListAsync(cancellationToken);
        return items.Adapt<List<ReceiverResponseDto>>();
    }
}

