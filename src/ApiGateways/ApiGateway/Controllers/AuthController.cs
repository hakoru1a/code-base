using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

/// <summary>
/// Authentication Controller - Proxy tới Auth Service
/// Gateway chỉ làm nhiệm vụ routing, không xử lý logic OAuth
/// </summary>
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// GET /auth/login
    /// Khởi tạo OAuth login flow thông qua Auth service
    /// </summary>
    [HttpGet("login")]
    public async Task<IActionResult> Login([FromQuery] string? returnUrl = null)
    {
        try
        {
            _logger.LogInformation("Login initiated, returnUrl: {ReturnUrl}", returnUrl);

            var authServiceUrl = _configuration["Services:AuthService:Url"] 
                ?? "http://localhost:5100";

            var client = _httpClientFactory.CreateClient();
            
            var response = await client.PostAsJsonAsync(
                $"{authServiceUrl}/api/auth/login/initiate",
                new { returnUrl });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Auth service error: {Error}", error);
                return StatusCode((int)response.StatusCode, error);
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            
            if (result == null || string.IsNullOrEmpty(result.AuthorizationUrl))
            {
                return StatusCode(500, new { error = "invalid_response" });
            }

            return Redirect(result.AuthorizationUrl);
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
            var authServiceUrl = _configuration["Services:AuthService:Url"] 
                ?? "http://localhost:5100";

            var client = _httpClientFactory.CreateClient();
            
            var response = await client.PostAsJsonAsync(
                $"{authServiceUrl}/api/auth/login/callback",
                new 
                { 
                    code, 
                    state, 
                    error, 
                    errorDescription 
                });

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Auth service callback error: {Error}", errorContent);
                return StatusCode((int)response.StatusCode, errorContent);
            }

            var result = await response.Content.ReadFromJsonAsync<SignInCallbackResponse>();

            if (result == null || string.IsNullOrEmpty(result.SessionId))
            {
                return StatusCode(500, new { error = "invalid_response" });
            }

            Response.Cookies.Append("session_id", result.SessionId, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromMinutes(480),
                Path = "/"
            });

            _logger.LogInformation("User logged in successfully, redirecting to: {RedirectUri}", 
                result.RedirectUri);

            return Redirect(result.RedirectUri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sign-in callback");
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
            if (!Request.Cookies.TryGetValue("session_id", out var sessionId) ||
                string.IsNullOrEmpty(sessionId))
            {
                return Ok(new { message = "No active session" });
            }

            var authServiceUrl = _configuration["Services:AuthService:Url"] 
                ?? "http://localhost:5100";

            var client = _httpClientFactory.CreateClient();
            
            await client.PostAsJsonAsync(
                $"{authServiceUrl}/api/auth/logout",
                new { sessionId });

            Response.Cookies.Delete("session_id", new CookieOptions { Path = "/" });

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
            if (!Request.Cookies.TryGetValue("session_id", out var sessionId) ||
                string.IsNullOrEmpty(sessionId))
            {
                return Unauthorized(new { error = "no_session", message = "Not logged in" });
            }

            var authServiceUrl = _configuration["Services:AuthService:Url"] 
                ?? "http://localhost:5100";

            var client = _httpClientFactory.CreateClient();
            
            var response = await client.GetAsync(
                $"{authServiceUrl}/api/auth/user/{sessionId}");

            if (!response.IsSuccessStatusCode)
            {
                return Unauthorized(new { error = "session_expired", message = "Session expired" });
            }

            var userInfo = await response.Content.ReadFromJsonAsync<UserInfoResponse>();

            return Ok(userInfo);
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

    private class LoginResponse
    {
        public string AuthorizationUrl { get; set; } = string.Empty;
    }

    private class SignInCallbackResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
    }

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
