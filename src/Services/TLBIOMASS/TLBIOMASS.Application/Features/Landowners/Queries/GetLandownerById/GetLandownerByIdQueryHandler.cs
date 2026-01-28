using MediatR;
using TLBIOMASS.Domain.Landowners.Interfaces;
using Shared.DTOs.Landowner;
using Mapster;

namespace TLBIOMASS.Application.Features.Landowners.Queries.GetLandownerById;

public class GetLandownerByIdQueryHandler : IRequestHandler<GetLandownerByIdQuery, LandownerResponseDto?>
{
    private readonly ILandownerRepository _repository;

    public GetLandownerByIdQueryHandler(ILandownerRepository repository)
    {
        _repository = repository;
    }

    public async Task<LandownerResponseDto?> Handle(GetLandownerByIdQuery request, CancellationToken cancellationToken)
    {
        var landowner = await _repository.GetByIdAsync(request.Id);
        return landowner?.Adapt<LandownerResponseDto>();
    }
}
