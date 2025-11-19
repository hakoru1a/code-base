using Contracts.Common.Interface;
using Contracts.Common.Events;
using MassTransit;
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
    /// Extension methods cho MassTransit - Message Bus cho Event-Driven Architecture
    /// 
    /// MỤC ĐÍCH:
    /// - Publish Domain Events tới Message Bus (RabbitMQ, Azure Service Bus, AWS SQS)
    /// - Send Commands tới specific queues/services
    /// - Hỗ trợ microservices communication async, loosely coupled
    /// 
    /// SỬ DỤNG:
    /// 1. Publish events (fan-out pattern):
    ///    await _publishEndpoint.PublishDomainEventsAsync(order.DomainEvents);
    ///    // Tất cả consumers đăng ký event này đều nhận được
    /// 
    /// 2. Send command (point-to-point):
    ///    await _sendEndpointProvider.SendCommandAsync(
    ///        new ProcessPaymentCommand { OrderId = 123 },
    ///        "payment-service-queue"
    ///    );
    /// 
    /// PATTERN:
    /// - Publish/Subscribe: 1 event → N consumers (notifications, analytics, logging)
    /// - Send/Receive: 1 command → 1 consumer (order processing, payment)
    /// 
    /// IMPACT:
    /// + Decoupling: Services không cần biết về nhau, chỉ cần biết events/commands
    /// + Scalability: Có thể scale consumers độc lập
    /// + Reliability: Message Bus đảm bảo delivery, retry nếu consumer fail
    /// + Async: Non-blocking, tăng throughput
    /// - Complexity: Debug khó hơn synchronous calls
    /// - Eventual Consistency: Data có thể inconsistent trong khoảng thời gian ngắn
    /// - Message Bus Dependency: Hệ thống phụ thuộc vào RabbitMQ/Service Bus availability
    /// </summary>
    public static class MassTransitExtension
    {
        /// <summary>
        /// Publish nhiều domain events cùng lúc
        /// 
        /// CÁCH DÙNG:
        /// var events = order.DomainEvents; // List<BaseEvent>
        /// await _publishEndpoint.PublishDomainEventsAsync(events);
        /// 
        /// PATTERN: Publish/Subscribe - Tất cả consumers quan tâm đều nhận được
        /// PHÙ HỢP: OrderCreatedEvent, ProductUpdatedEvent, UserRegisteredEvent
        /// </summary>
        public static async Task PublishDomainEventsAsync(this IPublishEndpoint publishEndpoint, List<BaseEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                await publishEndpoint.Publish(domainEvent);
            }
        }

        /// <summary>
        /// Publish một domain event duy nhất
        /// 
        /// CÁCH DÙNG:
        /// await _publishEndpoint.PublishDomainEventAsync(new OrderCreatedEvent { OrderId = 123 });
        /// 
        /// PATTERN: Publish/Subscribe
        /// </summary>
        public static async Task PublishDomainEventAsync(this IPublishEndpoint publishEndpoint, BaseEvent domainEvent)
        {
            await publishEndpoint.Publish(domainEvent);
        }

        /// <summary>
        /// Gửi command tới một queue cụ thể (theo tên queue)
        /// 
        /// CÁCH DÙNG:
        /// await _sendEndpointProvider.SendCommandAsync(
        ///     new ProcessPaymentCommand { OrderId = 123, Amount = 100 },
        ///     "payment-service-queue"
        /// );
        /// 
        /// PATTERN: Send/Receive (Point-to-Point)
        /// PHÙ HỢP: Commands cần xử lý bởi một service cụ thể
        /// </summary>
        public static async Task SendCommandAsync<T>(this ISendEndpointProvider sendEndpointProvider, T command, string queueName) where T : class
        {
            var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{queueName}"));
            await endpoint.Send(command);
        }

        /// <summary>
        /// Gửi command tới một endpoint cụ thể (theo URI)
        /// 
        /// CÁCH DÙNG:
        /// await _sendEndpointProvider.SendCommandAsync(
        ///     new ProcessPaymentCommand { OrderId = 123 },
        ///     new Uri("rabbitmq://localhost/payment-service-queue")
        /// );
        /// 
        /// PATTERN: Send/Receive (Point-to-Point)
        /// PHÙ HỢP: Khi cần control chi tiết endpoint address
        /// </summary>
        public static async Task SendCommandAsync<T>(this ISendEndpointProvider sendEndpointProvider, T command, Uri endpointAddress) where T : class
        {
            var endpoint = await sendEndpointProvider.GetSendEndpoint(endpointAddress);
            await endpoint.Send(command);
        }
    }
}
