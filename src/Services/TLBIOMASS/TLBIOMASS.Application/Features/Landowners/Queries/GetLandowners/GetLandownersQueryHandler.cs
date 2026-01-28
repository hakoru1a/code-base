using MediatR;
using TLBIOMASS.Domain.Landowners.Interfaces;
using Shared.DTOs.Landowner;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TLBIOMASS.Domain.Landowners;

namespace TLBIOMASS.Application.Features.Landowners.Queries.GetLandowners;

public class GetLandownersQueryHandler : IRequestHandler<GetLandownersQuery, List<LandownerResponseDto>>
{
    private readonly ILandownerRepository _repository;

    public GetLandownersQueryHandler(ILandownerRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<LandownerResponseDto>> Handle(GetLandownersQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll();

        if (!string.IsNullOrEmpty(request.Filter.SearchTerms))
        {
            var search = request.Filter.SearchTerms.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search) ||
                               (c.Contact != null && c.Contact.Phone != null && c.Contact.Phone.Contains(search)) ||
                               (c.Contact != null && c.Contact.Address != null && c.Contact.Address.ToLower().Contains(search)) ||
                               (c.Bank != null && c.Bank.BankAccount != null && c.Bank.BankAccount.Contains(search)) ||
                               (c.Bank != null && c.Bank.BankName != null && c.Bank.BankName.ToLower().Contains(search)) ||
                               (c.Identity != null && c.Identity.IdentityNumber != null && c.Identity.IdentityNumber.Contains(search)));
        }

        // Apply sorting (defaults to CreatedDate desc)
        query = query.OrderByDescending(x => x.CreatedDate);

        var items = await query.ToListAsync(cancellationToken);

        return items.Adapt<List<LandownerResponseDto>>();
    }
}
