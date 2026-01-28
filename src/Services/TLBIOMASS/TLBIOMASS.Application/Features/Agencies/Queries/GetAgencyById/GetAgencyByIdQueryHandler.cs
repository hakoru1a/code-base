using MediatR;
using TLBIOMASS.Domain.Agencies.Interfaces;
using Shared.DTOs.Agency;
using Contracts.Exceptions;
using Mapster;

namespace TLBIOMASS.Application.Features.Agencies.Queries.GetAgencyById;

public class GetAgencyByIdQueryHandler : IRequestHandler<GetAgencyByIdQuery, AgencyResponseDto?>
{
    private readonly IAgencyRepository _repository;

    public GetAgencyByIdQueryHandler(IAgencyRepository repository)
    {
        _repository = repository;
    }

    public async Task<AgencyResponseDto?> Handle(GetAgencyByIdQuery request, CancellationToken cancellationToken)
    {
        var agency = await _repository.GetByIdAsync(request.Id);
        if (agency == null)
        {
            throw new NotFoundException("Agency", request.Id);
        }
        return agency.Adapt<AgencyResponseDto>();
    }
}
