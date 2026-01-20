using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using ApiGateway.Configurations;
using ApiGateway.Services;
using ApiGateway.Models;
using Contracts.Identity;
using Shared.SeedWork;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace ApiGateway.Controllers;

/// <summary>
/// Authentication Controller - JWT-only approach
/// Không sử dụng session, cookie - chỉ trả JWT tokens
/// </summary>
[ApiController]
[Route("auth")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiErrorResult<object>), StatusCodes.Status500InternalServerError)]
public class AuthController : ControllerBase
{
    private readonly IPkceService _pkceService;
    private readonly IOAuthClient _oauthClient;
    private readonly ITemporaryTokenService _temporaryTokenService;
    private readonly IUserContextService _userContextService;
    private readonly OAuthOptions _oauthOptions;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IPkceService pkceService,
        IOAuthClient oauthClient,
        ITemporaryTokenService temporaryTokenService,
        IUserContextService userContextService,
        IOptions<OAuthOptions> oauthOptions,
        ILogger<AuthController> logger)
    {
        _pkceService = pkceService;
        _oauthClient = oauthClient;
        _temporaryTokenService = temporaryTokenService;
        _userContextService = userContextService;
        _oauthOptions = oauthOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// GET /auth/login
    /// Khởi tạo OAuth login flow
    /// </summary>
    [HttpGet("login")]
    [ProducesResponseType(302)]
    [ProducesResponseType(typeof(ApiErrorResult<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromQuery] string? returnUrl = null)
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        using var scope = _logger.BeginScope("CorrelationId: {CorrelationId}", correlationId);

        try
        {
            _logger.LogInformation("Login initiated, returnUrl: {ReturnUrl}, CorrelationId: {CorrelationId}", returnUrl, correlationId);

            var redirectUri = string.IsNullOrEmpty(returnUrl) ? _oauthOptions.WebAppUrl : returnUrl;
            var pkceData = await _pkceService.GeneratePkceAsync(redirectUri);
            var callbackUri = $"{Request.Scheme}://{Request.Host}{_oauthOptions.RedirectUri}";
            var authUrl = _oauthClient.BuildAuthorizationUrl(pkceData, callbackUri);

            _logger.LogInformation("Authorization URL generated, state: {State}, CorrelationId: {CorrelationId}", pkceData.State, correlationId);
            return Redirect(authUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login initiation failed, CorrelationId: {CorrelationId}", correlationId);
            return StatusCode(500, new ApiErrorResult<object>("Authentication service temporarily unavailable. Please try again."));
        }
    }

    /// <summary>
    /// GET /auth/signin-oidc
    /// Callback endpoint - Redirect về frontend với temporary code (an toàn)
    /// </summary>
    [HttpGet("signin-oidc")]
    [ProducesResponseType(302)]
    [ProducesResponseType(typeof(ApiErrorResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResult<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SignInCallback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        [FromQuery(Name = "error_description")] string? errorDescription)
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        using var scope = _logger.BeginScope("CorrelationId: {CorrelationId}", correlationId);

        try
        {
            // Xác định frontend URL để redirect
            string frontendUrl = _oauthOptions.WebAppUrl;

            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError("OAuth callback error: {Error}, Description: {Description}, CorrelationId: {CorrelationId}",
                    error, errorDescription, correlationId);

                // Redirect về frontend với error trong query string
                var errorRedirectUrl = $"{frontendUrl}/auth/callback?error={Uri.EscapeDataString(error)}&error_description={Uri.EscapeDataString(errorDescription ?? "Authentication failed")}";
                return Redirect(errorRedirectUrl);
            }

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                _logger.LogError("Missing required parameters in OAuth callback, CorrelationId: {CorrelationId}", correlationId);

                // Redirect về frontend với error
                var errorRedirectUrl = $"{frontendUrl}/auth/callback?error=missing_parameters&error_description={Uri.EscapeDataString("Missing required parameters")}";
                return Redirect(errorRedirectUrl);
            }

            _logger.LogInformation("Processing OAuth callback, state: {State}, CorrelationId: {CorrelationId}", state, correlationId);

            var pkceData = await _pkceService.GetAndRemovePkceAsync(state);
            if (pkceData == null)
            {
                _logger.LogError("PKCE validation failed for state: {State}, CorrelationId: {CorrelationId}", state, correlationId);

                // Redirect về frontend với error
                var errorRedirectUrl = $"{frontendUrl}/auth/callback?error=invalid_state&error_description={Uri.EscapeDataString("Invalid or expired authentication state")}";
                return Redirect(errorRedirectUrl);
            }

            // Sử dụng redirect URL từ PKCE nếu có, nếu không thì dùng WebAppUrl
            if (!string.IsNullOrEmpty(pkceData.RedirectUri))
            {
                try
                {
                    var uri = new Uri(pkceData.RedirectUri);
                    frontendUrl = $"{uri.Scheme}://{uri.Host}" + (uri.Port != 80 && uri.Port != 443 ? $":{uri.Port}" : "");
                }
                catch
                {
                    // Nếu không parse được URI, giữ nguyên WebAppUrl
                }
            }

            var callbackUri = $"{Request.Scheme}://{Request.Host}{_oauthOptions.RedirectUri}";
            var tokenResponse = await _oauthClient.ExchangeCodeForTokensAsync(code, pkceData.CodeVerifier, callbackUri);


            // Tạo temporary code và lưu token vào Redis
            var temporaryCode = await _temporaryTokenService.CreateTemporaryCodeAsync(tokenResponse, pkceData.RedirectUri);

            // Redirect về frontend với temporary code (không có token trong URL)
            var redirectUrl = $"{frontendUrl}/auth/callback?code={temporaryCode}";

            _logger.LogInformation("User authentication successful, redirecting to frontend with temporary code, CorrelationId: {CorrelationId}", correlationId);
            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth callback processing failed, CorrelationId: {CorrelationId}", correlationId);

            // Redirect về frontend với error
            var frontendUrl = _oauthOptions.WebAppUrl;
            var errorRedirectUrl = $"{frontendUrl}/auth/callback?error=server_error&error_description={Uri.EscapeDataString("Authentication processing failed. Please try again.")}";
            return Redirect(errorRedirectUrl);
        }
    }

    /// <summary>
    /// POST /auth/exchange
    /// Exchange temporary code for JWT tokens
    /// </summary>
    [HttpPost("exchange")]
    [ProducesResponseType(typeof(ApiSuccessResult<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResult<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExchangeToken([FromBody] ExchangeTokenRequest? request)
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        using var scope = _logger.BeginScope("CorrelationId: {CorrelationId}", correlationId);

        try
        {
            // Validation
            if (request == null)
            {
                _logger.LogWarning("Exchange token request is null, CorrelationId: {CorrelationId}", correlationId);
                return BadRequest(new ApiErrorResult<object>("Request body is required"));
            }

            if (string.IsNullOrWhiteSpace(request.Code))
            {
                _logger.LogWarning("Empty temporary code provided, CorrelationId: {CorrelationId}", correlationId);
                return BadRequest(new ApiErrorResult<object>("Temporary code is required"));
            }

            _logger.LogInformation("Processing token exchange request, CorrelationId: {CorrelationId}", correlationId);

            // Lấy và xóa token data từ Redis (one-time use)
            var tempTokenData = await _temporaryTokenService.GetAndRemoveTokenAsync(request.Code);
            if (tempTokenData == null)
            {
                _logger.LogWarning("Invalid or expired temporary code: {Code}, CorrelationId: {CorrelationId}", request.Code, correlationId);
                return NotFound(new ApiErrorResult<object>("Invalid or expired temporary code"));
            }

            var authResponse = new AuthResponse
            {
                AccessToken = tempTokenData.AccessToken,
                RefreshToken = tempTokenData.RefreshToken,
                TokenType = tempTokenData.TokenType,
                ExpiresIn = tempTokenData.ExpiresIn,
                RedirectUrl = tempTokenData.RedirectUrl
            };

            _logger.LogInformation("Token exchange successful, CorrelationId: {CorrelationId}", correlationId);
            return Ok(new ApiSuccessResult<AuthResponse>(authResponse, "Token exchange successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token exchange failed, CorrelationId: {CorrelationId}", correlationId);
            return StatusCode(500, new ApiErrorResult<object>("Token exchange failed. Please try again."));
        }
    }

    /// <summary>
    /// POST /auth/refresh
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiSuccessResult<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResult<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResult<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest? request)
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        using var scope = _logger.BeginScope("CorrelationId: {CorrelationId}", correlationId);

        try
        {
            // Validation
            if (request == null)
            {
                _logger.LogWarning("Refresh token request is null, CorrelationId: {CorrelationId}", correlationId);
                return BadRequest(new ApiErrorResult<object>("Request body is required"));
            }

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                _logger.LogWarning("Empty refresh token provided, CorrelationId: {CorrelationId}", correlationId);
                return BadRequest(new ApiErrorResult<object>("Refresh token is required"));
            }

            _logger.LogInformation("Processing token refresh request, CorrelationId: {CorrelationId}", correlationId);

            var tokenResponse = await _oauthClient.RefreshTokenAsync(request.RefreshToken);


            var authResponse = new AuthResponse
            {
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                TokenType = tokenResponse.TokenType,
                ExpiresIn = tokenResponse.ExpiresIn
            };

            _logger.LogInformation("Token refresh successful, CorrelationId: {CorrelationId}", correlationId);
            return Ok(new ApiSuccessResult<AuthResponse>(authResponse, "Token refreshed successfully"));
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("400"))
        {
            _logger.LogWarning(ex, "Invalid refresh token provided, CorrelationId: {CorrelationId}", correlationId);
            return Unauthorized(new ApiErrorResult<object>("Refresh token is invalid or expired"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed, CorrelationId: {CorrelationId}", correlationId);
            return StatusCode(500, new ApiErrorResult<object>("Token refresh failed. Please login again."));
        }
    }

    /// <summary>
    /// POST /auth/logout
    /// Revoke tokens và xóa cache
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiSuccessResult<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request)
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        using var scope = _logger.BeginScope("CorrelationId: {CorrelationId}", correlationId);

        try
        {
            if (request == null)
            {
                _logger.LogWarning("Logout request is null, CorrelationId: {CorrelationId}", correlationId);
                return BadRequest(new ApiErrorResult<object>("Request body is required"));
            }

            _logger.LogInformation("Processing logout request, CorrelationId: {CorrelationId}", correlationId);

            // Parallel execution for independent operations
            var tasks = new List<Task>();

            // Revoke refresh token if provided
            if (!string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                tasks.Add(RevokeTokenSafelyAsync(request.RefreshToken, correlationId));
            }

            // Note: ID token is not provided in LogoutRequest, so we can't end session at Keycloak
            // Client should call Keycloak logout endpoint directly if needed
            // Note: No need to remove cached claims since we're not using UserClaimsCache anymore
            // JWT tokens are stateless and will expire naturally

            await Task.WhenAll(tasks);

            _logger.LogInformation("User logout completed successfully, CorrelationId: {CorrelationId}", correlationId);
            return Ok(new ApiSuccessResult<object>(new { success = true }, "Logged out successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout, CorrelationId: {CorrelationId}", correlationId);
            return Ok(new ApiSuccessResult<object>(new { success = true }, "Logout completed with warnings"));
        }
    }

    /// <summary>
    /// GET /auth/user/{userId}
    /// Lấy cached user claims
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(ApiSuccessResult<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserClaims([Required] string userId)
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        using var scope = _logger.BeginScope("CorrelationId: {CorrelationId}", correlationId);

        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Empty userId provided, CorrelationId: {CorrelationId}", correlationId);
                return BadRequest(new ApiErrorResult<object>("User ID is required"));
            }

            // Check if requesting user's own data or has admin role
            var currentUserId = _userContextService.GetCurrentUserId();
            if (currentUserId != userId && !_userContextService.HasRole("admin"))
            {
                _logger.LogWarning("User {CurrentUserId} attempted to access claims for {RequestedUserId}, CorrelationId: {CorrelationId}",
                    currentUserId, userId, correlationId);
                return Forbid();
            }

            _logger.LogInformation("Retrieving user context for userId: {UserId}, CorrelationId: {CorrelationId}", userId, correlationId);

            var userContext = _userContextService.GetCurrentUser();

            _logger.LogInformation("User context retrieved successfully for userId: {UserId}, CorrelationId: {CorrelationId}", userId, correlationId);
            return Ok(new ApiSuccessResult<object>(userContext, "User context retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user claims for userId: {UserId}, CorrelationId: {CorrelationId}", userId, correlationId);
            return StatusCode(500, new ApiErrorResult<object>("Failed to retrieve user claims"));
        }
    }

    /// <summary>
    /// GET /auth/profile
    /// Lấy thông tin profile của user hiện tại từ JWT token
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiSuccessResult<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResult<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCurrentUserProfile()
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        using var scope = _logger.BeginScope("CorrelationId: {CorrelationId}", correlationId);

        try
        {
            // Get user context from JWT token
            if (!_userContextService.IsAuthenticated())
            {
                _logger.LogWarning("User is not authenticated, CorrelationId: {CorrelationId}", correlationId);
                return Unauthorized(new ApiErrorResult<object>("Invalid or missing authentication token"));
            }

            var userId = _userContextService.GetCurrentUserId();
            _logger.LogInformation("Retrieving profile for current user: {UserId}, CorrelationId: {CorrelationId}", userId, correlationId);

            // Get user context from JWT token (no cache needed)
            var userContext = _userContextService.GetCurrentUser();

            // Convert to UserProfileResponse format
            var profileResponse = UserProfileResponse.FromUserClaimsContext(userContext);

            _logger.LogInformation("User profile retrieved successfully for userId: {UserId}, CorrelationId: {CorrelationId}", userId, correlationId);
            return Ok(new ApiSuccessResult<UserProfileResponse>(profileResponse, "User profile retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user profile, CorrelationId: {CorrelationId}", correlationId);
            return StatusCode(500, new ApiErrorResult<object>("Failed to retrieve user profile"));
        }
    }

    /// <summary>
    /// GET /auth/health
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(ApiSuccessResult<object>), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        var healthData = new { status = "healthy", service = "auth-gateway", timestamp = DateTime.UtcNow };
        return Ok(new ApiSuccessResult<object>(healthData, "Service is healthy"));
    }

    #region Private Helper Methods

    /// <summary>
    /// Safely revoke refresh token with proper error handling
    /// </summary>
    private async Task RevokeTokenSafelyAsync(string refreshToken, string correlationId)
    {
        try
        {
            await _oauthClient.RevokeTokenAsync(refreshToken);
            _logger.LogInformation("Refresh token revoked successfully, CorrelationId: {CorrelationId}", correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to revoke refresh token, CorrelationId: {CorrelationId}", correlationId);
        }
    }

    /// <summary>
    /// Safely end session ở Keycloak với proper error handling
    /// </summary>
    private async Task EndSessionSafelyAsync(string idToken, string correlationId)
    {
        try
        {
            await _oauthClient.EndSessionAsync(idToken);
            _logger.LogInformation("Keycloak session ended successfully, CorrelationId: {CorrelationId}", correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to end Keycloak session, CorrelationId: {CorrelationId}", correlationId);
        }
    }

    /// <summary>
    /// Extract user ID từ JWT token trong HttpContext.User claims
    /// </summary>
    private string? GetUserIdFromToken()
    {
        try
        {
            // Lấy user ID từ "sub" claim (standard JWT claim cho subject/user ID)
            var userId = User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                // Fallback: thử các claim types khác
                userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting user ID from JWT token");
            return null;
        }
    }

    #endregion
}
