using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ApiGateway.Configurations;
using ApiGateway.Services;
using ApiGateway.Models;
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
    private readonly IUserClaimsCache _userClaimsCache;
    private readonly OAuthOptions _oauthOptions;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IPkceService pkceService,
        IOAuthClient oauthClient,
        IUserClaimsCache userClaimsCache,
        IOptions<OAuthOptions> oauthOptions,
        ILogger<AuthController> logger)
    {
        _pkceService = pkceService;
        _oauthClient = oauthClient;
        _userClaimsCache = userClaimsCache;
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
    /// Callback endpoint - Trả JWT tokens thay vì redirect
    /// </summary>
    [HttpGet("signin-oidc")]
    [ProducesResponseType(typeof(ApiSuccessResult<AuthResponse>), StatusCodes.Status200OK)]
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
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError("OAuth callback error: {Error}, Description: {Description}, CorrelationId: {CorrelationId}", 
                    error, errorDescription, correlationId);
                return BadRequest(new ApiErrorResult<object>(errorDescription ?? "Authentication failed"));
            }

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                _logger.LogError("Missing required parameters in OAuth callback, CorrelationId: {CorrelationId}", correlationId);
                return BadRequest(new ApiErrorResult<object>("Missing required parameters"));
            }

            _logger.LogInformation("Processing OAuth callback, state: {State}, CorrelationId: {CorrelationId}", state, correlationId);

            var pkceData = await _pkceService.GetAndRemovePkceAsync(state);
            if (pkceData == null)
            {
                _logger.LogError("PKCE validation failed for state: {State}, CorrelationId: {CorrelationId}", state, correlationId);
                return BadRequest(new ApiErrorResult<object>("Invalid or expired authentication state"));
            }

            var callbackUri = $"{Request.Scheme}://{Request.Host}{_oauthOptions.RedirectUri}";
            var tokenResponse = await _oauthClient.ExchangeCodeForTokensAsync(code, pkceData.CodeVerifier, callbackUri);

            // Cache user claims from ID token
            if (!string.IsNullOrEmpty(tokenResponse.IdToken))
            {
                await _userClaimsCache.CacheUserClaimsAsync(tokenResponse.IdToken);
            }

            var authResponse = new AuthResponse
            {
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                TokenType = tokenResponse.TokenType,
                ExpiresIn = tokenResponse.ExpiresIn,
                RedirectUrl = pkceData.RedirectUri
            };

            _logger.LogInformation("User authentication successful, CorrelationId: {CorrelationId}", correlationId);
            return Ok(new ApiSuccessResult<AuthResponse>(authResponse, "Authentication successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth callback processing failed, CorrelationId: {CorrelationId}", correlationId);
            return StatusCode(500, new ApiErrorResult<object>("Authentication processing failed. Please try again."));
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

            // Update cache if ID token available
            if (!string.IsNullOrEmpty(tokenResponse.IdToken))
            {
                await _userClaimsCache.CacheUserClaimsAsync(tokenResponse.IdToken);
            }

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

            if (!string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                tasks.Add(RevokeTokenSafelyAsync(request.RefreshToken, correlationId));
            }

            if (!string.IsNullOrWhiteSpace(request.UserId))
            {
                tasks.Add(_userClaimsCache.RemoveUserClaimsAsync(request.UserId));
            }

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
    [ProducesResponseType(typeof(ApiSuccessResult<CachedUserClaims>), StatusCodes.Status200OK)]
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

            _logger.LogInformation("Retrieving user claims for userId: {UserId}, CorrelationId: {CorrelationId}", userId, correlationId);

            var userClaims = await _userClaimsCache.GetUserClaimsAsync(userId);

            if (userClaims == null)
            {
                _logger.LogInformation("User claims not found for userId: {UserId}, CorrelationId: {CorrelationId}", userId, correlationId);
                return NotFound(new ApiErrorResult<object>("User claims not found in cache"));
            }

            _logger.LogInformation("User claims retrieved successfully for userId: {UserId}, CorrelationId: {CorrelationId}", userId, correlationId);
            return Ok(new ApiSuccessResult<CachedUserClaims>(userClaims, "User claims retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user claims for userId: {UserId}, CorrelationId: {CorrelationId}", userId, correlationId);
            return StatusCode(500, new ApiErrorResult<object>("Failed to retrieve user claims"));
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

    #endregion
}
