using System.Linq;
using MediatR;
using TLBIOMASS.Domain.Materials.Interfaces;
using Shared.DTOs.Material;
using Shared.SeedWork;
using Mapster;
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
        var query = _repository.FindAll();

        query = ApplyFilter(query, request.Filter);

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

        query = ApplySort(query, request.Filter.OrderBy, request.Filter.OrderByDirection);

        var pagedItems = await _repository.GetPageAsync(query, request.Filter.PageNumber, request.Filter.PageSize, cancellationToken);

        return new PagedList<MaterialResponseDto>(
            pagedItems.Adapt<List<MaterialResponseDto>>(),
            pagedItems.GetMetaData().TotalItems,
            request.Filter.PageNumber, request.Filter.PageSize);
    }

    private static IQueryable<Material> ApplyFilter(IQueryable<Material> query, MaterialPagedFilterDto filter)
    {
        if (filter == null) return query;

        if (filter.IsActive.HasValue)
            query = query.Where(x => x.IsActive == filter.IsActive.Value);

        return query;
    }

    private static IQueryable<Material> ApplySort(IQueryable<Material> query, string? orderBy, string? direction)
    {
        return query;
    }
}
