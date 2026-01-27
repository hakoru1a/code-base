using MediatR;
using Shared.DTOs.Material;

namespace TLBIOMASS.Application.Features.Materials.Queries.GetAllMaterials;

public class GetAllMaterialsQuery : IRequest<List<MaterialResponseDto>>
{
    public MaterialFilterDto Filter { get; set; } = new();
}
