using MediatR;
using Shared.DTOs.MaterialRegion;

namespace TLBIOMASS.Application.Features.MaterialRegions.Queries.GetMaterialRegionById
{
    public class GetMaterialRegionByIdQuery : IRequest<MaterialRegionResponseDto>
    {
        public int Id { get; set; }
    }
}
