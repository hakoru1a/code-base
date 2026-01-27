using MediatR;
using TLBIOMASS.Application.Features.Agencies.DTOs;

namespace TLBIOMASS.Application.Features.Agencies.Queries.GetAgencyById
{
    public class GetAgencyByIdQuery : IRequest<AgencyResponseDto>
    {
        public int Id { get; set; }
    }
}
