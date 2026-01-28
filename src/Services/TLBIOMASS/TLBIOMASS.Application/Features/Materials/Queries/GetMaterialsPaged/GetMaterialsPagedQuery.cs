using MediatR;
using Shared.DTOs.Material;
using Shared.SeedWork;

namespace TLBIOMASS.Application.Features.Materials.Queries.GetMaterialsPaged;

public class GetMaterialsPagedQuery : IRequest<PagedList<MaterialResponseDto>>
{
    public MaterialPagedFilterDto Filter { get; set; } = new();
}

