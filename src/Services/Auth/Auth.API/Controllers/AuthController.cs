using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Configurations;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers;

/// <summary>
/// Auth Service Controller
/// Xử lý tất cả logic authentication với Keycloak
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IPkceService _pkceService;
    private readonly ISessionManager _sessionManager;
    private readonly IOAuthClient _oauthClient;
    private readonly OAuthSettings _oauthSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IPkceService pkceService,
        ISessionManager sessionManager,
        IOAuthClient oauthClient,
        OAuthSettings oauthSettings,
        ILogger<AuthController> logger)
    {
        _pkceService = pkceService;
        _sessionManager = sessionManager;
        _oauthClient = oauthClient;
        _oauthSettings = oauthSettings;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/auth/login/initiate
    /// Khởi tạo OAuth login flow và trả về authorization URL
    /// </summary>
    [HttpPost("login/initiate")]
    public async Task<ActionResult<LoginResponse>> InitiateLogin([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login initiate request, returnUrl: {ReturnUrl}", request.ReturnUrl);

            var redirectUri = string.IsNullOrEmpty(request.ReturnUrl)
                ? _oauthSettings.WebAppUrl
                : request.ReturnUrl;

            var pkceData = await _pkceService.GeneratePkceAsync(redirectUri);

            // Callback URI là URL của gateway, không phải auth service
            var callbackUri = _oauthSettings.RedirectUri;
            var authUrl = _oauthClient.BuildAuthorizationUrl(pkceData, callbackUri);

            _logger.LogInformation("Authorization URL generated, state: {State}", pkceData.State);

            return Ok(new LoginResponse { AuthorizationUrl = authUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating login");
            return StatusCode(500, new { error = "login_failed", message = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/auth/login/callback
    /// Xử lý callback sau khi user login thành công ở Keycloak
    /// </summary>
    [HttpPost("login/callback")]
    public async Task<ActionResult<SignInCallbackResponse>> ProcessCallback([FromBody] SignInCallbackRequest request)
    {
        try
        {
            if (!string.IsNullOrEmpty(request.Error))
            {
                _logger.LogError("OAuth error: {Error}, Description: {Description}", 
                    request.Error, request.ErrorDescription);

                return BadRequest(new
                {
                    error = request.Error,
                    message = request.ErrorDescription ?? "Authentication failed"
                });
            }

            if (string.IsNullOrEmpty(request.Code) || string.IsNullOrEmpty(request.State))
            {
                _logger.LogError("Missing code or state parameter");
                return BadRequest(new
                {
                    error = "invalid_callback",
                    message = "Missing required parameters"
                });
            }

            _logger.LogInformation("Processing callback, state: {State}", request.State);

            var pkceData = await _pkceService.GetAndRemovePkceAsync(request.State);

            if (pkceData == null)
            {
                _logger.LogError("PKCE data not found or expired for state: {State}", request.State);
                return BadRequest(new
                {
                    error = "invalid_state",
                    message = "Invalid or expired state parameter"
                });
            }

            var callbackUri = _oauthSettings.RedirectUri;
            var tokenResponse = await _oauthClient.ExchangeCodeForTokensAsync(
                request.Code,
                pkceData.CodeVerifier,
                callbackUri);

            var sessionId = await _sessionManager.CreateSessionAsync(tokenResponse);

            _logger.LogInformation("User logged in successfully, session: {SessionId}", sessionId);

            return Ok(new SignInCallbackResponse
            {
                SessionId = sessionId,
                RedirectUri = pkceData.RedirectUri
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing callback");
            return StatusCode(500, new { error = "callback_failed", message = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/auth/logout
    /// Logout user
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SessionId))
            {
                return BadRequest(new { error = "invalid_request", message = "SessionId is required" });
            }

            var session = await _sessionManager.GetSessionAsync(request.SessionId);

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

            await _sessionManager.RemoveSessionAsync(request.SessionId);

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
    /// GET /api/auth/user/{sessionId}
    /// Lấy thông tin user từ session
    /// </summary>
    [HttpGet("user/{sessionId}")]
    public async Task<ActionResult<UserInfoResponse>> GetUserInfo(string sessionId)
    {
        try
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest(new { error = "invalid_request", message = "SessionId is required" });
            }

            var session = await _sessionManager.GetSessionAsync(sessionId);

            if (session == null)
            {
                return NotFound(new { error = "session_not_found", message = "Session not found or expired" });
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
            _logger.LogError(ex, "Error getting user info");
            return StatusCode(500, new { error = "get_user_failed", message = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/auth/session/{sessionId}/validate
    /// Validate session và trả về access token
    /// </summary>
    [HttpGet("session/{sessionId}/validate")]
    public async Task<ActionResult<SessionValidationResponse>> ValidateSession(string sessionId)
    {
        try
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return Ok(new SessionValidationResponse { IsValid = false });
            }

            var session = await _sessionManager.GetSessionAsync(sessionId);

            if (session == null)
            {
                return Ok(new SessionValidationResponse { IsValid = false });
            }

            // Check if token needs refresh
            if (session.NeedsRefresh())
            {
                try
                {
                    var tokenResponse = await _oauthClient.RefreshTokenAsync(session.RefreshToken);
                    
                    session.AccessToken = tokenResponse.AccessToken;
                    session.RefreshToken = tokenResponse.RefreshToken ?? session.RefreshToken;
                    session.ExpiresAt = tokenResponse.CalculateExpiresAt();

                    await _sessionManager.UpdateSessionAsync(session);

                    _logger.LogInformation("Token refreshed for session: {SessionId}", sessionId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to refresh token for session: {SessionId}", sessionId);
                    return Ok(new SessionValidationResponse { IsValid = false });
                }
            }

            return Ok(new SessionValidationResponse
            {
                IsValid = true,
                AccessToken = session.AccessToken,
                ExpiresAt = session.ExpiresAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating session");
            return Ok(new SessionValidationResponse { IsValid = false });
        }
    }

    /// <summary>
    /// GET /api/auth/health
    /// Health check
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "auth-api" });
    }
}
