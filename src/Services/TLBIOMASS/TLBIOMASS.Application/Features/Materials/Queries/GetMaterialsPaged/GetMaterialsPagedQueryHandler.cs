using System.Linq;
using MediatR;
using TLBIOMASS.Domain.Materials.Interfaces;
using Shared.DTOs.Material;
using Shared.SeedWork;
using Mapster;
using TLBIOMASS.Domain.Materials;

namespace TLBIOMASS.Application.Features.Materials.Queries.GetMaterialsPaged;

public class GetMaterialsPagedQueryHandler : IRequestHandler<GetMaterialsPagedQuery, PagedList<MaterialResponseDto>>
{
    private readonly IMaterialRepository _repository;

    public GetMaterialsPagedQueryHandler(IMaterialRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<MaterialResponseDto>> Handle(GetMaterialsPagedQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll();

        query = ApplyFilter(query, request.Filter);

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

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(x => x.Spec.Name.ToLower().Contains(search) || 
                               (x.Spec.Description != null && x.Spec.Description.ToLower().Contains(search)));
        }

        return query;
    }

    private static IQueryable<Material> ApplySort(IQueryable<Material> query, string? orderBy, string? direction)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
            return query.OrderBy(x => x.Id);

        var isDescending = direction?.ToLower() == "desc";

        return orderBy.ToLower() switch
        {
            "name" => isDescending
                ? query.OrderByDescending(x => x.Spec.Name)
                : query.OrderBy(x => x.Spec.Name),
            "unit" => isDescending
                ? query.OrderByDescending(x => x.Spec.Unit)
                : query.OrderBy(x => x.Spec.Unit),
            "description" => isDescending
                ? query.OrderByDescending(x => x.Spec.Description)
                : query.OrderBy(x => x.Spec.Description),
            "isactive" => isDescending
                ? query.OrderByDescending(x => x.IsActive)
                : query.OrderBy(x => x.IsActive),
            "createdat" => isDescending
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt),
            "updatedat" => isDescending
                ? query.OrderByDescending(x => x.UpdatedAt)
                : query.OrderBy(x => x.UpdatedAt),
            _ => query.OrderBy(x => x.Id)
        };
    }
}

