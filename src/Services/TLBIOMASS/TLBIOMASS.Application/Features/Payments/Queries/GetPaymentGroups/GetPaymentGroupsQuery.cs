using MediatR;
using Shared.DTOs.Payment;
using Shared.SeedWork;

namespace TLBIOMASS.Application.Features.Payments.Queries.GetPaymentGroups;

public class GetPaymentGroupsQuery : IRequest<PagedList<PaymentGroupResponseDto>>
{
    public PaymentDetailFilterDto Filter { get; set; } = new();
}
