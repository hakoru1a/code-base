using MediatR;
using Shared.DTOs.Payment;

namespace TLBIOMASS.Application.Features.Payments.Queries.GetPaymentDetailById;

public class GetPaymentDetailByIdQuery : IRequest<PaymentDetailResponseDto>
{
    public int Id { get; set; }

    public GetPaymentDetailByIdQuery(int id)
    {
        Id = id;
    }
}
