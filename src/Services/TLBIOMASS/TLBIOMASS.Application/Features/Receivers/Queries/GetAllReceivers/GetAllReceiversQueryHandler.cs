using MediatR;
using Shared.DTOs.Receiver;
using TLBIOMASS.Domain.Receivers.Interfaces;
using TLBIOMASS.Domain.Receivers.Specifications;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TLBIOMASS.Domain.Receivers;

namespace TLBIOMASS.Application.Features.Receivers.Queries.GetAllReceivers;

public class GetAllReceiversQueryHandler : IRequestHandler<GetAllReceiversQuery, List<ReceiverResponseDto>>
{
    private readonly IReceiverRepository _repository;

    public GetAllReceiversQueryHandler(IReceiverRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ReceiverResponseDto>> Handle(GetAllReceiversQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll();

        if (!string.IsNullOrEmpty(request.Filter.Search))
        {
            var spec = new ReceiverSearchSpecification(request.Filter.Search);
            query = query.Where(spec.ToExpression());
        }

        if (request.Filter.IsActive.HasValue)
        {
            var spec = new ReceiverIsActiveSpecification(request.Filter.IsActive.Value);
            query = query.Where(spec.ToExpression());
        }

        var items = await query.ToListAsync(cancellationToken);

        return items.Adapt<List<ReceiverResponseDto>>();
    }

}
