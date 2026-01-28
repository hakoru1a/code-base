using MediatR;
using TLBIOMASS.Domain.Landowners.Interfaces;
using Shared.DTOs.Landowner;
using Shared.SeedWork;
using Mapster;
using System.Linq.Expressions;
using TLBIOMASS.Domain.Landowners;
using TLBIOMASS.Domain.Landowners.Specifications;
using System.Linq;

namespace TLBIOMASS.Application.Features.Landowners.Queries.GetLandowners;

public class GetLandownersQueryHandler : IRequestHandler<GetLandownersQuery, PagedList<LandownerResponseDto>>
{
    private readonly ILandownerRepository _repository;

    public GetLandownersQueryHandler(ILandownerRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<LandownerResponseDto>> Handle(GetLandownersQuery request, CancellationToken cancellationToken)
    {
        // Start with base query
        var query = _repository.FindAll();

        // Apply filters using Specifications
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

        // Get paginated results
        var pagedItems = await _repository.GetPageAsync(query, request.Filter.PageNumber, request.Filter.PageSize, cancellationToken);

        // Map to DTOs and return PagedList
        return new PagedList<LandownerResponseDto>(
            pagedItems.Adapt<List<LandownerResponseDto>>(),
            pagedItems.GetMetaData().TotalItems,
            request.Filter.PageNumber, request.Filter.PageSize);

    }


}
