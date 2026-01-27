using MediatR;
using TLBIOMASS.Domain.Materials.Interfaces;
using Shared.DTOs.Material;
using Shared.SeedWork;
using Mapster;
using System.Linq;
using TLBIOMASS.Domain.Materials;
using TLBIOMASS.Domain.Materials.Specifications;

namespace TLBIOMASS.Application.Features.Materials.Queries.GetMaterials;

public class GetMaterialsQueryHandler : IRequestHandler<GetMaterialsQuery, PagedList<MaterialResponseDto>>
{
    private readonly IMaterialRepository _repository;

    public GetMaterialsQueryHandler(IMaterialRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<MaterialResponseDto>> Handle(GetMaterialsQuery request, CancellationToken cancellationToken)
    {
        // Start with base query
        var query = _repository.FindAll();

        // Apply filters
        if (!string.IsNullOrEmpty(request.Filter.Search))
        {
            var spec = new MaterialSearchSpecification(request.Filter.Search);
            query = query.Where(spec.ToExpression());
        }

        if (request.Filter.IsActive.HasValue)
        {
            var spec = new MaterialIsActiveSpecification(request.Filter.IsActive.Value);
            query = query.Where(spec.ToExpression());
        }

        // Apply sorting
        query = ApplySorting(query, request.Filter.OrderBy, request.Filter.OrderByDirection);

        // Get paginated results
        var pagedItems = await _repository.GetPageAsync(query, request.Filter.PageNumber, request.Filter.PageSize, cancellationToken);

        // Map to DTOs
        return new PagedList<MaterialResponseDto>(
            pagedItems.Adapt<List<MaterialResponseDto>>(),
            pagedItems.GetMetaData().TotalItems,
            request.Filter.PageNumber, request.Filter.PageSize);

    }

    private IQueryable<Material> ApplySorting(IQueryable<Material> query, string? sortBy, string? sortDirection)
    {
        var isDescending = sortDirection?.ToLower() == "desc";

        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderByDescending(x => x.CreatedAt);
        }

        return sortBy.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "unit" => isDescending ? query.OrderByDescending(x => x.Unit) : query.OrderBy(x => x.Unit),
            "createdat" or "created" => isDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            "updatedat" or "updated" => isDescending ? query.OrderByDescending(x => x.UpdatedAt) : query.OrderBy(x => x.UpdatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };
    }
}
