using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TLBIOMASS.API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Public endpoint - không yêu cầu đăng nhập
    /// </summary>
    /// <returns>Public message</returns>
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult GetPublicMessage()
    {
        try
        {
            var response = new
            {
                Message = "This is a public endpoint - no authentication required",
                Timestamp = DateTime.UtcNow,
                Status = "Success"
            };

            _logger.LogInformation("Public endpoint accessed at {Timestamp}", DateTime.UtcNow);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in public endpoint");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Protected endpoint - yêu cầu đăng nhập và trả về thông tin user
    /// </summary>
    /// <returns>User information from claims</returns>
    [HttpGet("protected")]
    [Authorize]
    public IActionResult GetUserInfo()
    {
        try
        {
            // Lấy thông tin user từ claims
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        HttpContext.User.FindFirst("sub")?.Value ??
                        HttpContext.User.FindFirst("user_id")?.Value;

            var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ??
                          HttpContext.User.FindFirst("preferred_username")?.Value ??
                          HttpContext.User.FindFirst("username")?.Value;

            var email = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value ??
                       HttpContext.User.FindFirst("email")?.Value;

            var roles = HttpContext.User.FindAll(ClaimTypes.Role)
                                      .Select(c => c.Value)
                                      .ToList();

            // Lấy tất cả claims để debug
            var allClaims = HttpContext.User.Claims
                                          .Select(c => new { Type = c.Type, Value = c.Value })
                                          .ToList();

            var response = new
            {
                Message = "This is a protected endpoint - authentication required",
                UserInfo = new
                {
                    UserId = userId,
                    Username = username,
                    Email = email,
                    Roles = roles
                },
                AllClaims = allClaims,
                Timestamp = DateTime.UtcNow,
                Status = "Success"
            };

            _logger.LogInformation("Protected endpoint accessed by user {UserId} ({Username}) at {Timestamp}",
                userId, username, DateTime.UtcNow);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in protected endpoint");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new
        {
            Status = "Healthy",
            Service = "TLBIOMASS.API",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        });
    }
}