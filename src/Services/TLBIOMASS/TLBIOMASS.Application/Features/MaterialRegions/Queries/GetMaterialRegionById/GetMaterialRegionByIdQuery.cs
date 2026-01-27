using MediatR;
using TLBIOMASS.Application.Features.MaterialRegions.DTOs;

namespace TLBIOMASS.Application.Features.MaterialRegions.Queries.GetMaterialRegionById
{
    public class GetMaterialRegionByIdQuery : IRequest<MaterialRegionResponseDto>
    {
        public int Id { get; set; }
    }
}
