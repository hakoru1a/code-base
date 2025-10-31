using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Services
{
    public interface INotificationService
    {
        string ProviderName { get; }

        // --- Send single push ---
        Task<string> SendAsync(string token, string title, string body, IDictionary<string, string>? data = null, string? channel = null, string priority = "normal", CancellationToken ct = default);

        // --- Send multiple push (batch) ---
        Task<IReadOnlyList<string>> SendManyAsync(IEnumerable<string> tokens, string title, string body, IDictionary<string, string>? data = null, string? channel = null, string priority = "normal", CancellationToken ct = default);

        // --- Publish topic (broadcast) ---
        Task<string> PublishToTopicAsync(string topic, string title, string body, IDictionary<string, string>? data = null, CancellationToken ct = default);

        // --- Topic management ---
        Task<bool> SubscribeTopicAsync(string token, string topic, CancellationToken ct = default);
        Task<bool> UnsubscribeTopicAsync(string token, string topic, CancellationToken ct = default);

        // --- Capability check ---
        Task<bool> CanDeliverAsync(string token, string channel, CancellationToken ct = default);
        Task<IDictionary<string, bool>> CanDeliverManyAsync(IEnumerable<string> tokens, string channel, CancellationToken ct = default);
    }

}
