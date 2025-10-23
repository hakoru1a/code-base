using Contracts.Services;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class FcmNotificationService : IFcmNotification
    {
        private readonly FcmSettings _settings;
        private readonly FirebaseMessaging _messaging;

        public string ProviderName => "Firebase Cloud Messaging";

        public FcmNotificationService(IOptions<FcmSettings> options)
        {
            _settings = options.Value;

            // Initialize Firebase App if not already initialized
            if (FirebaseApp.DefaultInstance == null)
            {
                var credential = GoogleCredential.FromFile(_settings.CredentialPath);
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = credential,
                    ProjectId = _settings.ProjectId
                });
            }

            _messaging = FirebaseMessaging.DefaultInstance;
        }

        public async Task<string> SendAsync(string token, string title, string body, IDictionary<string, string>? data = null, string? channel = null, string priority = "normal", CancellationToken ct = default)
        {
            var message = new Message
            {
                Token = token,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data != null ? new Dictionary<string, string>(data) : null,
                Android = new AndroidConfig
                {
                    Priority = priority.ToLower() == "high" ? Priority.High : Priority.Normal,
                    Notification = new AndroidNotification
                    {
                        ChannelId = channel
                    }
                },
                Apns = new ApnsConfig
                {
                    Headers = new Dictionary<string, string>
                    {
                        ["apns-priority"] = priority.ToLower() == "high" ? "10" : "5"
                    }
                }
            };

            var response = await _messaging.SendAsync(message, ct);
            return response;
        }

        public async Task<IReadOnlyList<string>> SendManyAsync(IEnumerable<string> tokens, string title, string body, IDictionary<string, string>? data = null, string? channel = null, string priority = "normal", CancellationToken ct = default)
        {
            var tokenList = tokens.ToList();
            var messages = tokenList.Select(token => new Message
            {
                Token = token,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data != null ? new Dictionary<string, string>(data) : null,
                Android = new AndroidConfig
                {
                    Priority = priority.ToLower() == "high" ? Priority.High : Priority.Normal,
                    Notification = new AndroidNotification
                    {
                        ChannelId = channel
                    }
                },
                Apns = new ApnsConfig
                {
                    Headers = new Dictionary<string, string>
                    {
                        ["apns-priority"] = priority.ToLower() == "high" ? "10" : "5"
                    }
                }
            }).ToList();

            var response = await _messaging.SendEachAsync(messages, ct);
            var successfulTokens = new List<string>();

            for (int i = 0; i < response.Responses.Count; i++)
            {
                if (response.Responses[i].IsSuccess)
                {
                    successfulTokens.Add(tokenList[i]);
                }
            }

            return successfulTokens;
        }

        public async Task<string> PublishToTopicAsync(string topic, string title, string body, IDictionary<string, string>? data = null, CancellationToken ct = default)
        {
            var message = new Message
            {
                Topic = topic,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data != null ? new Dictionary<string, string>(data) : null
            };

            var response = await _messaging.SendAsync(message, ct);
            return response;
        }

        public async Task<bool> SubscribeTopicAsync(string token, string topic, CancellationToken ct = default)
        {
            try
            {
                var response = await _messaging.SubscribeToTopicAsync(new List<string> { token }, topic);
                return response.SuccessCount > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UnsubscribeTopicAsync(string token, string topic, CancellationToken ct = default)
        {
            try
            {
                var response = await _messaging.UnsubscribeFromTopicAsync(new List<string> { token }, topic);
                return response.SuccessCount > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CanDeliverAsync(string token, string channel, CancellationToken ct = default)
        {
            try
            {
                // Send a dry-run message to check if the token is valid
                var message = new Message
                {
                    Token = token,
                    Notification = new Notification
                    {
                        Title = "Test",
                        Body = "Test"
                    }
                };

                await _messaging.SendAsync(message, dryRun: true, ct);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IDictionary<string, bool>> CanDeliverManyAsync(IEnumerable<string> tokens, string channel, CancellationToken ct = default)
        {
            var results = new Dictionary<string, bool>();
            var tokenList = tokens.ToList();

            var messages = tokenList.Select(token => new Message
            {
                Token = token,
                Notification = new Notification
                {
                    Title = "Test",
                    Body = "Test"
                }
            }).ToList();

            try
            {
                var response = await _messaging.SendEachAsync(messages, dryRun: true, ct);

                for (int i = 0; i < tokenList.Count; i++)
                {
                    results[tokenList[i]] = response.Responses[i].IsSuccess;
                }
            }
            catch
            {
                foreach (var token in tokenList)
                {
                    results[token] = false;
                }
            }

            return results;
        }
    }
}
