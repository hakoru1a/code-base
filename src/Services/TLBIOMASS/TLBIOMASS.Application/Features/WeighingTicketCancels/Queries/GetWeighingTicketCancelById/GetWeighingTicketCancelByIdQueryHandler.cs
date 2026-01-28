using MediatR;
using Shared.DTOs.WeighingTicketCancel;
using TLBIOMASS.Domain.WeighingTicketCancels.Interfaces;
using Contracts.Exceptions;
using Mapster;

namespace TLBIOMASS.Application.Features.WeighingTicketCancels.Queries.GetWeighingTicketCancelById;

public class GetWeighingTicketCancelByIdQueryHandler : IRequestHandler<GetWeighingTicketCancelByIdQuery, WeighingTicketCancelResponseDto>
{
    private readonly IWeighingTicketCancelRepository _repository;

    public GetWeighingTicketCancelByIdQueryHandler(IWeighingTicketCancelRepository repository)
    {
        _repository = repository;
    }

    public async Task<WeighingTicketCancelResponseDto> Handle(GetWeighingTicketCancelByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        if (entity == null)
            throw new NotFoundException("WeighingTicketCancel", request.Id);

        return entity.Adapt<WeighingTicketCancelResponseDto>();
    }
}
