using Contracts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class FcmNotificationService : IFcmNotification
    {
        public string ProviderName => throw new NotImplementedException();

        public Task<bool> CanDeliverAsync(string token, string channel, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, bool>> CanDeliverManyAsync(IEnumerable<string> tokens, string channel, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> PublishToTopicAsync(string topic, string title, string body, IDictionary<string, string>? data = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> SendAsync(string token, string title, string body, IDictionary<string, string>? data = null, string? channel = null, string priority = "normal", CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<string>> SendManyAsync(IEnumerable<string> tokens, string title, string body, IDictionary<string, string>? data = null, string? channel = null, string priority = "normal", CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SubscribeTopicAsync(string token, string topic, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UnsubscribeTopicAsync(string token, string topic, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
