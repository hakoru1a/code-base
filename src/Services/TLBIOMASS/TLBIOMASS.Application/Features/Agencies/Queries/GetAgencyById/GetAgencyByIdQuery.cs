using MediatR;
using Shared.DTOs.Agency;

namespace TLBIOMASS.Application.Features.Agencies.Queries.GetAgencyById
{
    public class GetAgencyByIdQuery : IRequest<AgencyResponseDto>
    {
        public int Id { get; set; }
    }
}
