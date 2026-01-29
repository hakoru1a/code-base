using MediatR;
using Shared.DTOs.Payment;
using TLBIOMASS.Domain.Payments.Interfaces;
using Microsoft.EntityFrameworkCore;
using Mapster;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.Payments.Queries.GetPaymentDetailById;

public class GetPaymentDetailByIdQueryHandler : IRequestHandler<GetPaymentDetailByIdQuery, PaymentDetailResponseDto>
{
    private readonly IPaymentDetailRepository _repository;

    public GetPaymentDetailByIdQueryHandler(IPaymentDetailRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaymentDetailResponseDto> Handle(GetPaymentDetailByIdQuery request, CancellationToken cancellationToken)
    {
        var paymentDetail = await _repository.FindByCondition(x => x.Id == request.Id)
            .Include(x => x.WeighingTicket)
            .Include(x => x.Agency)
            .ProjectToType<PaymentDetailResponseDto>()
            .FirstOrDefaultAsync(cancellationToken);

        if (paymentDetail == null)
        {
            throw new NotFoundException("PaymentDetail", request.Id);
        }

        return paymentDetail;
    }
}
