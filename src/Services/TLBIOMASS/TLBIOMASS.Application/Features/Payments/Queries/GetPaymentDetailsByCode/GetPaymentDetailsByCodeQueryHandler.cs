using MediatR;
using Shared.DTOs.Payment;
using TLBIOMASS.Domain.Payments.Interfaces;
using Microsoft.EntityFrameworkCore;
using Mapster;
using System.Collections.Generic;

namespace TLBIOMASS.Application.Features.Payments.Queries.GetPaymentDetailsByCode;

public class GetPaymentDetailsByCodeQueryHandler : IRequestHandler<GetPaymentDetailsByCodeQuery, List<PaymentDetailResponseDto>>
{
    private readonly IPaymentDetailRepository _repository;

    public GetPaymentDetailsByCodeQueryHandler(IPaymentDetailRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PaymentDetailResponseDto>> Handle(GetPaymentDetailsByCodeQuery request, CancellationToken cancellationToken)
    {
        return await _repository.FindByCondition(x => x.PaymentCode == request.PaymentCode)
            .Include(x => x.WeighingTicket)
            .Include(x => x.Agency)
            .ProjectToType<PaymentDetailResponseDto>()
            .ToListAsync(cancellationToken);
    }
}
