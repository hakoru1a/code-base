using MediatR;
using TLBIOMASS.Domain.Payments;
using TLBIOMASS.Domain.Payments.Interfaces;
using TLBIOMASS.Domain.Payments.Rules;
using TLBIOMASS.Domain.WeighingTickets.Interfaces;
using Contracts.Exceptions;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs.Payment;

namespace TLBIOMASS.Application.Features.Payments.Commands.CreatePayment;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, List<PaymentResultDto>>
{
    private readonly IPaymentDetailRepository _paymentDetailRepository;
    private readonly IWeighingTicketPaymentRepository _paymentConfigRepository;
    private readonly IWeighingTicketRepository _ticketRepository;

    public CreatePaymentCommandHandler(
        IPaymentDetailRepository paymentDetailRepository,
        IWeighingTicketPaymentRepository paymentConfigRepository,
        IWeighingTicketRepository ticketRepository)
    {
        _paymentDetailRepository = paymentDetailRepository;
        _paymentConfigRepository = paymentConfigRepository;
        _ticketRepository = ticketRepository;
    }

    public async Task<List<PaymentResultDto>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        if (request.Items == null || !request.Items.Any())
            return new List<PaymentResultDto>();

        var ticketIds = request.Items.Select(x => x.WeighingTicketId).Distinct().ToList();
        
        // 1. Fetch all tickets and existing configs in bulk
        var tickets = await _ticketRepository.FindAll()
            .Where(x => ticketIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var existingConfigs = await _paymentConfigRepository.FindAll()
            .Where(x => ticketIds.Contains(x.WeighingTicketId))
            .ToDictionaryAsync(x => x.WeighingTicketId, cancellationToken);

        // 2. Fetch latest payment details to calculate remaining amounts
        var allDetails = await _paymentDetailRepository.FindAll()
            .Where(x => ticketIds.Contains(x.WeighingTicketId))
            .OrderByDescending(x => x.CreatedDate)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

        var latestDetailsMap = allDetails
            .GroupBy(x => x.WeighingTicketId)
            .ToDictionary(g => g.Key, g => g.First());

        var newDetails = new List<PaymentDetail>();
        var newConfigs = new List<WeighingTicketPayment>();
        var results = new List<PaymentResultDto>();

        foreach (var item in request.Items)
        {
            if (!tickets.TryGetValue(item.WeighingTicketId, out var ticket))
                throw new NotFoundException("WeighingTicket", item.WeighingTicketId);

            // Business Rule Validation: Must have a Receiver assigned
            var receiverRule = new ReceiverMustBeAssignedBeforePaymentRule(ticket.ReceiverId);
            if (receiverRule.IsBroken())
                throw new BadRequestException(receiverRule.Message);

            // 3. Handle Payment Configuration (Final Price/Total)
            WeighingTicketPayment config;
            if (existingConfigs.TryGetValue(item.WeighingTicketId, out var existingConfig))
            {
                config = existingConfig;
                
                // Business Rule: Price cannot be changed after chốt (finalized)
                if (item.FinalUnitPrice.HasValue && item.FinalUnitPrice.Value != (config.UnitPrice ?? 0))
                {
                    var priceRule = new PriceCannotBeChangedAfterPaymentRule(true);
                    throw new BadRequestException($"Ticket {item.WeighingTicketId}: {priceRule.Message}");
                }
            }
            else
            {
                // First time finalizing price
                decimal finalUnitPrice = item.FinalUnitPrice ?? (decimal)ticket.Price;
                decimal finalTotalAmount = item.FinalTotalPayableAmount ?? (decimal)ticket.TotalAmount;

                config = WeighingTicketPayment.Create(
                    item.WeighingTicketId,
                    finalUnitPrice,
                    finalTotalAmount,
                    "Finalized during first payment",
                    alreadyExists: false);
                
                newConfigs.Add(config);
                existingConfigs[item.WeighingTicketId] = config; // Track in memory for batch
            }

            // 4. Calculate current remaining
            decimal currentRemaining;
            if (latestDetailsMap.TryGetValue(item.WeighingTicketId, out var latest))
            {
                currentRemaining = latest.PaymentAmount.RemainingAmount ?? 0;
            }
            else
            {
                currentRemaining = config.TotalPayableAmount ?? 0;
            }

            // 5. Create Detail
            var detail = PaymentDetail.Create(
                item.WeighingTicketId,
                item.AgencyId,
                new TLBIOMASS.Domain.Payments.ValueObjects.PaymentInfo(request.PaymentCode, request.PaymentDate, item.CustomerPaymentDate, item.Note),
                item.Amount,
                currentRemaining,
                item.IsPaid);

            newDetails.Add(detail);
            
            // Track result (remaining amount after this specific payment)
            results.Add(new PaymentResultDto 
            { 
                WeighingTicketId = item.WeighingTicketId, 
                RemainingAmount = detail.PaymentAmount.RemainingAmount ?? 0 
            });

            // Update tracking for multiple payments to same ticket in single batch
            latestDetailsMap[item.WeighingTicketId] = detail;
        }

        // 6. Persistence
        if (newConfigs.Any())
        {
            await _paymentConfigRepository.CreateListAsync(newConfigs, cancellationToken);
        }

        if (newDetails.Any())
        {
            await _paymentDetailRepository.CreateListAsync(newDetails, cancellationToken);
        }

        await _paymentDetailRepository.SaveChangesAsync(cancellationToken);

        return results;
    }
}
