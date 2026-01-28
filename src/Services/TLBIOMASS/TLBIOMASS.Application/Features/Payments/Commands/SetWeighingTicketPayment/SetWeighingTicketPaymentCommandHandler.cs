using MediatR;
using TLBIOMASS.Domain.Payments;
using TLBIOMASS.Domain.Payments.Interfaces;
using TLBIOMASS.Domain.WeighingTickets.Interfaces;
using Contracts.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace TLBIOMASS.Application.Features.Payments.Commands.SetWeighingTicketPayment;

public class SetWeighingTicketPaymentCommandHandler : IRequestHandler<SetWeighingTicketPaymentCommand, int>
{
    private readonly IWeighingTicketPaymentRepository _paymentConfigRepository;
    private readonly IPaymentDetailRepository _paymentDetailRepository;
    private readonly IWeighingTicketRepository _ticketRepository;

    public SetWeighingTicketPaymentCommandHandler(
        IWeighingTicketPaymentRepository paymentConfigRepository,
        IPaymentDetailRepository paymentDetailRepository,
        IWeighingTicketRepository ticketRepository)
    {
        _paymentConfigRepository = paymentConfigRepository;
        _paymentDetailRepository = paymentDetailRepository;
        _ticketRepository = ticketRepository;
    }

    public async Task<int> Handle(SetWeighingTicketPaymentCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.WeighingTicketId);
        if (ticket == null)
            throw new NotFoundException("WeighingTicket", request.WeighingTicketId);

        var existingConfig = await _paymentConfigRepository.FindAll()
            .FirstOrDefaultAsync(x => x.WeighingTicketId == request.WeighingTicketId, cancellationToken);

        // If user didn't provide price, take from ticket
        decimal unitPrice = request.UnitPrice ?? ticket.Price;
        decimal totalAmount = request.TotalPayableAmount ?? ticket.TotalAmount;

        if (existingConfig == null)
        {
            var newConfig = WeighingTicketPayment.Create(
                request.WeighingTicketId,
                unitPrice,
                totalAmount,
                request.Note,
                alreadyExists: false);

            await _paymentConfigRepository.CreateAsync(newConfig);
            await _paymentConfigRepository.SaveChangesAsync(cancellationToken);
            return newConfig.Id;
        }
        else
        {
            // Check if any payments have been made
            var hasPayments = await _paymentDetailRepository.FindAll()
                .AnyAsync(x => x.WeighingTicketId == request.WeighingTicketId, cancellationToken);

            existingConfig.Update(unitPrice, totalAmount, request.Note, hasPayments);
            
            await _paymentConfigRepository.UpdateAsync(existingConfig);
            await _paymentConfigRepository.SaveChangesAsync(cancellationToken);
            return existingConfig.Id;
        }
    }
}
