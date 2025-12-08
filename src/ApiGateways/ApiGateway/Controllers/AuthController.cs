using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ApiGateway.Configurations;
using ApiGateway.Services;
using ApiGateway.Models;

namespace ApiGateway.Controllers;

/// <summary>
/// Authentication Controller - Xử lý OAuth flow trực tiếp tại Gateway
/// </summary>
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IPkceService _pkceService;
    private readonly ISessionManager _sessionManager;
    private readonly IOAuthClient _oauthClient;
    private readonly OAuthOptions _oauthOptions;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IPkceService pkceService,
        ISessionManager sessionManager,
        IOAuthClient oauthClient,
        IOptions<OAuthOptions> oauthOptions,
        ILogger<AuthController> logger)
    {
        _pkceService = pkceService;
        _sessionManager = sessionManager;
        _oauthClient = oauthClient;
        _oauthOptions = oauthOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// GET /auth/login
    /// Khởi tạo OAuth login flow
    /// </summary>
    [HttpGet("login")]
    public async Task<IActionResult> Login([FromQuery] string? returnUrl = null)
    {
        try
        {
            _logger.LogInformation("Login initiated, returnUrl: {ReturnUrl}", returnUrl);

            var redirectUri = string.IsNullOrEmpty(returnUrl)
                ? _oauthOptions.WebAppUrl
                : returnUrl;

            var pkceData = await _pkceService.GeneratePkceAsync(redirectUri);

            // Callback URI là URL của gateway
            var callbackUri = $"{Request.Scheme}://{Request.Host}{_oauthOptions.RedirectUri}";
            var authUrl = _oauthClient.BuildAuthorizationUrl(pkceData, callbackUri);

            _logger.LogInformation("Authorization URL generated, state: {State}", pkceData.State);

            return Redirect(authUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating login");
            return StatusCode(500, new { error = "login_failed", message = ex.Message });
        }
    }

    /// <summary>
    /// GET /auth/signin-oidc
    /// Callback endpoint sau khi user login thành công ở Keycloak
    /// </summary>
    [HttpGet("signin-oidc")]
    public async Task<IActionResult> SignInCallback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        [FromQuery(Name = "error_description")] string? errorDescription)
    {
        try
        {
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError("OAuth error: {Error}, Description: {Description}",
                    error, errorDescription);

                return BadRequest(new
                {
                    error,
                    message = errorDescription ?? "Authentication failed"
                });
            }

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                _logger.LogError("Missing code or state parameter");
                return BadRequest(new
                {
                    error = "invalid_callback",
                    message = "Missing required parameters"
                });
            }

            _logger.LogInformation("Processing callback, state: {State}", state);

            var pkceData = await _pkceService.GetAndRemovePkceAsync(state);

            if (pkceData == null)
            {
                _logger.LogError("PKCE data not found or expired for state: {State}", state);
                return BadRequest(new
                {
                    error = "invalid_state",
                    message = "Invalid or expired state parameter"
                });
            }

            var callbackUri = $"{Request.Scheme}://{Request.Host}{_oauthOptions.RedirectUri}";
            var tokenResponse = await _oauthClient.ExchangeCodeForTokensAsync(
                code,
                pkceData.CodeVerifier,
                callbackUri);

            var sessionId = await _sessionManager.CreateSessionAsync(tokenResponse);

            Response.Cookies.Append(CookieConstants.SessionIdCookieName, sessionId, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromMinutes(CookieConstants.SessionMaxAgeMinutes),
                Path = CookieConstants.CookiePath
            });

            _logger.LogInformation("User logged in successfully, session: {SessionId}", sessionId);

            return Redirect(pkceData.RedirectUri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing callback");
            return StatusCode(500, new { error = "callback_failed", message = ex.Message });
        }
    }

    /// <summary>
    /// POST /auth/logout
    /// Logout user
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            if (!Request.Cookies.TryGetValue(CookieConstants.SessionIdCookieName, out var sessionId) ||
                string.IsNullOrEmpty(sessionId))
            {
                return Ok(new { message = "No active session" });
            }

            var session = await _sessionManager.GetSessionAsync(sessionId);

            if (session != null)
            {
                try
                {
                    await _oauthClient.RevokeTokenAsync(session.RefreshToken);
                    _logger.LogInformation("Tokens revoked for user: {Username}", session.Username);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to revoke tokens, continuing logout");
                }
            }

            await _sessionManager.RemoveSessionAsync(sessionId);

            Response.Cookies.Delete(CookieConstants.SessionIdCookieName, new CookieOptions
            {
                Path = CookieConstants.CookiePath
            });

            _logger.LogInformation("User logged out successfully");

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { error = "logout_failed", message = ex.Message });
        }
    }

    /// <summary>
    /// GET /auth/user
    /// Lấy thông tin user hiện tại
    /// </summary>
    [HttpGet("user")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            if (!Request.Cookies.TryGetValue(CookieConstants.SessionIdCookieName, out var sessionId) ||
                string.IsNullOrEmpty(sessionId))
            {
                return Unauthorized(new { error = "no_session", message = "Not logged in" });
            }

            var session = await _sessionManager.GetSessionAsync(sessionId);

            if (session == null)
            {
                return Unauthorized(new { error = "session_expired", message = "Session expired" });
            }

            return Ok(new UserInfoResponse
            {
                UserId = session.UserId,
                Username = session.Username,
                Email = session.Email,
                Roles = session.Roles,
                Claims = session.Claims
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { error = "get_user_failed", message = ex.Message });
        }
    }

    /// <summary>
    /// GET /auth/health
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "auth-gateway" });
    }

    #region DTOs

    private class UserInfoResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public Dictionary<string, string> Claims { get; set; } = new();
    }

    #endregion
}
