using ApiGateway.Configurations;
using ApiGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

/// <summary>
/// Authentication Controller
/// Handles OAuth 2.0 Authorization Code Flow + PKCE
/// Endpoints:
/// - GET /auth/login: Khởi tạo login flow
/// - GET /auth/signin-oidc: Callback sau khi login
/// - POST /auth/logout: Logout
/// - GET /auth/user: Lấy thông tin user hiện tại
/// </summary>
[ApiController]
[Route("auth")]
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
    /// GET /auth/login
    /// Khởi tạo OAuth login flow
    /// Flow:
    /// 1. Tạo PKCE data (code_verifier, code_challenge, state)
    /// 2. Lưu vào Redis
    /// 3. Redirect browser tới Keycloak login page với code_challenge
    /// </summary>
    /// <param name="returnUrl">URL để redirect sau khi login thành công</param>
    [HttpGet("login")]
    public async Task<IActionResult> Login([FromQuery] string? returnUrl = null)
    {
        try
        {
            _logger.LogInformation("Login initiated, returnUrl: {ReturnUrl}", returnUrl);

            // 1. Determine redirect URI
            var redirectUri = string.IsNullOrEmpty(returnUrl)
                ? _oauthSettings.WebAppUrl
                : returnUrl;

            // 2. Tạo PKCE data
            var pkceData = await _pkceService.GeneratePkceAsync(redirectUri);

            // 3. Build authorization URL
            var callbackUri = $"{Request.Scheme}://{Request.Host}{_oauthSettings.RedirectUri}";
            var authUrl = _oauthClient.BuildAuthorizationUrl(pkceData, callbackUri);

            _logger.LogInformation(
                "Redirecting to Keycloak for login, state: {State}",
                pkceData.State);

            // 4. Redirect browser tới Keycloak
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
    /// Flow:
    /// 1. Validate state (CSRF protection)
    /// 2. Lấy PKCE data từ Redis
    /// 3. Exchange authorization code để lấy tokens
    /// 4. Tạo session và lưu vào Redis
    /// 5. Set HttpOnly cookie với session_id
    /// 6. Redirect về webapp
    /// </summary>
    /// <param name="code">Authorization code từ Keycloak</param>
    /// <param name="state">State parameter (CSRF protection)</param>
    [HttpGet("signin-oidc")]
    public async Task<IActionResult> SignInCallback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        [FromQuery(Name = "error_description")] string? errorDescription)
    {
        try
        {
            // 1. Kiểm tra error từ Keycloak
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError(
                    "OAuth error from Keycloak: {Error}, Description: {Description}",
                    error,
                    errorDescription);

                return BadRequest(new
                {
                    error,
                    message = errorDescription ?? "Authentication failed"
                });
            }

            // 2. Validate required parameters
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                _logger.LogError("Missing code or state parameter");
                return BadRequest(new
                {
                    error = "invalid_callback",
                    message = "Missing required parameters"
                });
            }

            _logger.LogInformation("Sign-in callback received, state: {State}", state);

            // 3. Lấy và validate PKCE data
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

            // 4. Exchange authorization code để lấy tokens
            var callbackUri = $"{Request.Scheme}://{Request.Host}{_oauthSettings.RedirectUri}";
            var tokenResponse = await _oauthClient.ExchangeCodeForTokensAsync(
                code,
                pkceData.CodeVerifier,
                callbackUri);

            // 5. Tạo session và lưu vào Redis
            var sessionId = await _sessionManager.CreateSessionAsync(tokenResponse);

            // 6. Set HttpOnly cookie với session_id
            // HttpOnly: không thể access từ JavaScript -> an toàn hơn
            // Secure: chỉ gửi qua HTTPS (production)
            // SameSite=Lax: CSRF protection
            Response.Cookies.Append("session_id", sessionId, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps, // true nếu HTTPS
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromMinutes(_oauthSettings.SessionAbsoluteExpirationMinutes),
                Path = "/"
            });

            _logger.LogInformation(
                "User logged in successfully, redirecting to: {RedirectUri}",
                pkceData.RedirectUri);

            // 7. Redirect về webapp
            return Redirect(pkceData.RedirectUri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sign-in callback");
            return StatusCode(500, new
            {
                error = "callback_failed",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// POST /auth/logout
    /// Logout user
    /// Flow:
    /// 1. Lấy session từ cookie
    /// 2. Revoke tokens ở Keycloak
    /// 3. Xóa session khỏi Redis
    /// 4. Delete session cookie
    /// 5. Redirect về Keycloak logout hoặc webapp
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // 1. Lấy session ID từ cookie
            if (Request.Cookies.TryGetValue("session_id", out var sessionId) &&
                !string.IsNullOrEmpty(sessionId))
            {
                // 2. Lấy session để có refresh token
                var session = await _sessionManager.GetSessionAsync(sessionId);

                if (session != null)
                {
                    // 3. Revoke tokens ở Keycloak
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

                // 4. Xóa session khỏi Redis
                await _sessionManager.RemoveSessionAsync(sessionId);
            }

            // 5. Delete session cookie
            Response.Cookies.Delete("session_id", new CookieOptions
            {
                Path = "/"
            });

            _logger.LogInformation("User logged out successfully");

            // 6. Return success (webapp sẽ handle redirect)
            return Ok(new
            {
                message = "Logged out successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new
            {
                error = "logout_failed",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// GET /auth/user
    /// Lấy thông tin user hiện tại từ session
    /// </summary>
    [HttpGet("user")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            // 1. Lấy session từ cookie
            if (!Request.Cookies.TryGetValue("session_id", out var sessionId) ||
                string.IsNullOrEmpty(sessionId))
            {
                return Unauthorized(new
                {
                    error = "no_session",
                    message = "Not logged in"
                });
            }

            // 2. Lấy session từ Redis
            var session = await _sessionManager.GetSessionAsync(sessionId);

            if (session == null)
            {
                return Unauthorized(new
                {
                    error = "session_expired",
                    message = "Session expired"
                });
            }

            // 3. Return user info (KHÔNG return tokens ra client!)
            return Ok(new
            {
                userId = session.UserId,
                username = session.Username,
                email = session.Email,
                roles = session.Roles,
                claims = session.Claims
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new
            {
                error = "get_user_failed",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// GET /auth/health
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "auth" });
    }
}

