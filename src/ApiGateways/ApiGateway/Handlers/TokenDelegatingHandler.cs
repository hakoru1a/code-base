using ApiGateway.Configurations;

namespace ApiGateway.Handlers;

/// <summary>
/// DelegatingHandler để tự động thêm Access Token vào downstream requests
/// Ocelot sẽ dùng handler này để inject Bearer token vào Authorization header
/// khi forward requests tới backend services
/// </summary>
public class TokenDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TokenDelegatingHandler> _logger;

    public TokenDelegatingHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<TokenDelegatingHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext != null)
        {
            // 1. Lấy access token từ HttpContext.Items
            // (được set bởi SessionValidationMiddleware)
            if (httpContext.Items.TryGetValue(HttpContextItemKeys.AccessToken, out var tokenObj) &&
                tokenObj is string accessToken &&
                !string.IsNullOrEmpty(accessToken))
            {
                // 2. Thêm Bearer token vào Authorization header
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        AuthenticationConstants.BearerScheme, accessToken);

                _logger.LogDebug(
                    "Added Bearer token to downstream request: {Method} {Uri}",
                    request.Method,
                    request.RequestUri);
            }
            else
            {
                _logger.LogWarning(
                    "No access token found for downstream request: {Method} {Uri}",
                    request.Method,
                    request.RequestUri);
            }
        }

        // 3. Continue với request
        return await base.SendAsync(request, cancellationToken);
    }
}

