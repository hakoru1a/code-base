using MediatR;
using Shared.DTOs.Payment;

namespace TLBIOMASS.Application.Features.Payments.Commands.CreatePayment;

public class CreatePaymentCommand : IRequest<List<PaymentResultDto>>
{
    public string PaymentCode { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public List<PaymentItemDto> Items { get; set; } = new();
}
