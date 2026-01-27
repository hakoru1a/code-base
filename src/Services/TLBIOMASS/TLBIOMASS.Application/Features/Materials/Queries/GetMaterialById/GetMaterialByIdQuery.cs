using MediatR;
using TLBIOMASS.Application.Features.Materials.DTOs;

namespace TLBIOMASS.Application.Features.Materials.Queries.GetMaterialById
{
    public class GetMaterialByIdQuery : IRequest<MaterialResponseDto>
    {
        public int Id { get; set; }
    }
}
