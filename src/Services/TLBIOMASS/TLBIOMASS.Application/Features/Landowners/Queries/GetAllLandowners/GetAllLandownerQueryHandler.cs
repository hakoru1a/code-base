using MediatR;
using TLBIOMASS.Domain.Landowners.Interfaces;
using Shared.DTOs.Landowner;
using TLBIOMASS.Domain.Landowners.Specifications;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TLBIOMASS.Domain.Landowners;

namespace TLBIOMASS.Application.Features.Landowners.Queries.GetAllLandowners;

public class GetAllLandownersQueryHandler : IRequestHandler<GetAllLandownersQuery, List<LandownerResponseDto>>
{
    private readonly ILandownerRepository _repository;

    public GetAllLandownersQueryHandler(ILandownerRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<LandownerResponseDto>> Handle(GetAllLandownersQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll();

        if (!string.IsNullOrEmpty(request.Filter.Search))
        {
            var spec = new LandownerSearchSpecification(request.Filter.Search);
            query = query.Where(spec.ToExpression());
        }

        if (request.Filter.IsActive.HasValue)
        {
            var spec = new LandownerIsActiveSpecification(request.Filter.IsActive.Value);
            query = query.Where(spec.ToExpression());
        }

        // Apply sorting (defaults to CreatedDate desc)
        query = query.OrderByDescending(x => x.CreatedDate);

        var items = await query.ToListAsync(cancellationToken);

        return items.Adapt<List<LandownerResponseDto>>();
    }
}
