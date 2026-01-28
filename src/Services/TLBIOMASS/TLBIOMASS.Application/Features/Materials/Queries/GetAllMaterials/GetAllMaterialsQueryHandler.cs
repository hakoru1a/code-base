using MediatR;
using TLBIOMASS.Domain.Materials.Interfaces;
using Shared.DTOs.Material;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TLBIOMASS.Domain.Materials.Specifications;
using System.Collections.Generic;
using System.Linq;

namespace TLBIOMASS.Application.Features.Materials.Queries.GetAllMaterials;

public class GetAllMaterialsQueryHandler : IRequestHandler<GetAllMaterialsQuery, List<MaterialResponseDto>>
{
    private readonly IMaterialRepository _repository;

    public GetAllMaterialsQueryHandler(IMaterialRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MaterialResponseDto>> Handle(GetAllMaterialsQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll();

        // 1. Apply Filters
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

        // 2. Fetch and Adapt (No sorting)
        var entities = await query.ToListAsync(cancellationToken);
        return entities.Adapt<List<MaterialResponseDto>>();
    }
}
