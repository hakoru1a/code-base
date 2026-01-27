using MediatR;
using TLBIOMASS.Application.Features.Receivers.DTOs;
using TLBIOMASS.Domain.Receivers.Interfaces;
using Mapster;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.Receivers.Queries.GetReceiverById;

public class GetReceiverByIdQueryHandler : IRequestHandler<GetReceiverByIdQuery, ReceiverResponseDto>
{
    private readonly IReceiverRepository _repository;

    public GetReceiverByIdQueryHandler(IReceiverRepository repository)
    {
        _repository = repository;
    }

    public async Task<ReceiverResponseDto> Handle(GetReceiverByIdQuery request, CancellationToken cancellationToken)
    {
        var receiver = await _repository.GetByIdAsync(request.Id);

        if (receiver == null)
        {
            throw new NotFoundException("Receiver", request.Id);
        }

        return receiver.Adapt<ReceiverResponseDto>();
    }
}
