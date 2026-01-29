using MediatR;
using Shared.DTOs.Payment;
using System.Collections.Generic;

namespace TLBIOMASS.Application.Features.Payments.Queries.GetPaymentDetailsByCode;

public class GetPaymentDetailsByCodeQuery : IRequest<List<PaymentDetailResponseDto>>
{
    public string PaymentCode { get; set; }

    public GetPaymentDetailsByCodeQuery(string paymentCode)
    {
        PaymentCode = paymentCode;
    }
}
