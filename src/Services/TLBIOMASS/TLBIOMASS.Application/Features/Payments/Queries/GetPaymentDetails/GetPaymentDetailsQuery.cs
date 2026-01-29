using MediatR;
using Shared.DTOs.Payment;
using Shared.SeedWork;

namespace TLBIOMASS.Application.Features.Payments.Queries.GetPaymentDetails;

public class GetPaymentDetailsQuery : IRequest<PagedList<PaymentDetailResponseDto>>
{
    public PaymentDetailFilterDto Filter { get; set; } = new();
}
