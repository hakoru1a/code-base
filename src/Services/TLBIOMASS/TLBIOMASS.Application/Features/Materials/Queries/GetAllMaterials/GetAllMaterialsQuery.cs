using MediatR;
using TLBIOMASS.Application.Features.Materials.DTOs;

namespace TLBIOMASS.Application.Features.Materials.Queries.GetAllMaterials;

public class GetAllMaterialsQuery : IRequest<List<MaterialResponseDto>>
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
