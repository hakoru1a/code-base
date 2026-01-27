using MediatR;
using TLBIOMASS.Domain.Materials.Interfaces;
using TLBIOMASS.Application.Features.Materials.DTOs;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TLBIOMASS.Domain.Materials;
using TLBIOMASS.Domain.Materials.Specifications;

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

        if (!string.IsNullOrEmpty(request.Search))
        {
            var spec = new MaterialSearchSpecification(request.Search);
            query = query.Where(spec.ToExpression());
        }

        if (request.IsActive.HasValue)
        {
            var spec = new MaterialIsActiveSpecification(request.IsActive.Value);
            query = query.Where(spec.ToExpression());
        }

        // Default sorting
        query = query.OrderByDescending(x => x.CreatedAt);

        var items = await query.ToListAsync(cancellationToken);

        return items.Adapt<List<MaterialResponseDto>>();
    }
}
