using MediatR;

namespace TLBIOMASS.Application.Features.Payments.Commands.SetWeighingTicketPayment;

public class SetWeighingTicketPaymentCommand : IRequest<int>
{
    public int WeighingTicketId { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPayableAmount { get; set; }
    public string? Note { get; set; }
}
