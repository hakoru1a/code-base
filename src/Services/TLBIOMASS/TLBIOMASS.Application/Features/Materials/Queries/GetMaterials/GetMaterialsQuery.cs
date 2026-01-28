using MediatR;
using Shared.DTOs.Material;

namespace TLBIOMASS.Application.Features.Materials.Queries.GetMaterials;

public class GetMaterialsQuery : IRequest<List<MaterialResponseDto>>
{
    public MaterialFilterDto Filter { get; set; } = new();
}

