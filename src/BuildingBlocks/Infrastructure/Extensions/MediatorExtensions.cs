using Contracts.Common.Interface;
using Contracts.Common.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Interfaces.Event;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods cho MediatR - In-process mediator pattern
    /// 
    /// MỤC ĐÍCH:
    /// - Dispatch Domain Events trong cùng process/service (không qua Message Bus)
    /// - Hỗ trợ CQRS pattern: tách biệt Commands và Queries
    /// - Decoupling: Handlers không cần biết về nhau
    /// 
    /// SỬ DỤNG:
    /// await _mediator.DispatchDomainEventAsync(aggregate.DomainEvents);
    /// 
    /// SO SÁNH MediatR vs MassTransit:
    /// - MediatR: In-process, same service, synchronous/async
    ///   → Dùng cho: Update cache, validate business rules, side effects trong service
    /// 
    /// - MassTransit: Out-of-process, cross-services, async qua Message Bus
    ///   → Dùng cho: Notify other services, send emails, analytics
    /// 
    /// VÍ DỤ:
    /// // Domain event được raise khi Order được tạo
    /// order.AddDomainEvent(new OrderCreatedDomainEvent(order));
    /// 
    /// // Trong SaveChanges, dispatch tất cả domain events:
    /// await _mediator.DispatchDomainEventAsync(order.DomainEvents);
    /// 
    /// // Handler in-process (cùng service):
    /// public class OrderCreatedDomainEventHandler : INotificationHandler<OrderCreatedDomainEvent>
    /// {
    ///     public async Task Handle(OrderCreatedDomainEvent notification)
    ///     {
    ///         // Update inventory cache
    ///         // Send notification to user
    ///         // Log analytics event
    ///     }
    /// }
    /// 
    /// IMPACT:
    /// + Performance: In-process, nhanh hơn Message Bus
    /// + Simplicity: Không cần external dependencies (RabbitMQ, Redis)
    /// + Transaction: Cùng transaction với main operation
    /// - Limited Scope: Chỉ trong cùng service, không thể notify services khác
    /// - Synchronous Impact: Handlers chậm = main operation chậm
    /// </summary>
    public static class MediatorExtention
    {
        /// <summary>
        /// Dispatch nhiều domain events qua MediatR (in-process)
        /// 
        /// CÁCH DÙNG:
        /// var domainEvents = order.DomainEvents; // List<BaseEvent>
        /// await _mediator.DispatchDomainEventAsync(domainEvents);
        /// 
        /// PATTERN: Mediator pattern - All handlers trong service này sẽ được gọi
        /// PHÙ HỢP: Side effects cần xử lý ngay trong cùng transaction
        /// 
        /// LƯU Ý: 
        /// - Tất cả handlers chạy tuần tự (sequential)
        /// - Nếu một handler throw exception, các handlers sau không chạy
        /// - Nên dùng MassTransit nếu muốn async cross-service communication
        /// </summary>
        public static async Task DispatchDomainEventAsync(this IMediator mediator, List<BaseEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                await mediator.Publish(domainEvent);
            }
        }
    }
}
