using MediatR;
using Shared.DTOs.Material;
using Shared.SeedWork;

namespace TLBIOMASS.Application.Features.Materials.Queries.GetMaterials;

public class GetMaterialsQuery : IRequest<PagedList<MaterialResponseDto>>
{
    public MaterialPagedFilterDto Filter { get; set; } = new();
}
