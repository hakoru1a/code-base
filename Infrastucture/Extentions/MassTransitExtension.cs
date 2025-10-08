using Constracts.Common.Interface;
using Contracts.Common.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    public static class MassTransitExtension
    {
        public static async Task PublishDomainEventsAsync(this IPublishEndpoint publishEndpoint, List<BaseEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                await publishEndpoint.Publish(domainEvent);
            }
        }

        public static async Task PublishDomainEventAsync(this IPublishEndpoint publishEndpoint, BaseEvent domainEvent)
        {
            await publishEndpoint.Publish(domainEvent);
        }

        public static async Task SendCommandAsync<T>(this ISendEndpointProvider sendEndpointProvider, T command, string queueName) where T : class
        {
            var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{queueName}"));
            await endpoint.Send(command);
        }

        public static async Task SendCommandAsync<T>(this ISendEndpointProvider sendEndpointProvider, T command, Uri endpointAddress) where T : class
        {
            var endpoint = await sendEndpointProvider.GetSendEndpoint(endpointAddress);
            await endpoint.Send(command);
        }
    }
}
