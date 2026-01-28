using MediatR;
using Shared.DTOs.Material;

namespace TLBIOMASS.Application.Features.Materials.Queries.GetMaterialById
{
    public class GetMaterialByIdQuery : IRequest<MaterialResponseDto>
    {
        public int Id { get; set; }
    }
}
