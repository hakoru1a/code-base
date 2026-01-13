using System.Security.Cryptography;
using System.Text;

namespace ApiGateway.Services;

/// <summary>
/// Service để tạo và validate client fingerprint
/// Giúp bind session với specific client để tăng cường bảo mật
/// </summary>
public interface IClientFingerprintService
{
    /// <summary>
    /// Tạo fingerprint từ HTTP context
    /// </summary>
    string GenerateFingerprint(HttpContext context);

    /// <summary>
    /// Validate fingerprint với context hiện tại
    /// </summary>
    bool ValidateFingerprint(string storedFingerprint, HttpContext context);
}

public class ClientFingerprintService : IClientFingerprintService
{
    private readonly ILogger<ClientFingerprintService> _logger;

    public ClientFingerprintService(ILogger<ClientFingerprintService> logger)
    {
        _logger = logger;
    }

    public string GenerateFingerprint(HttpContext context)
    {
        try
        {
            // Chỉ dùng User-Agent để tránh vấn đề IP thay đổi (load balancer/proxy)
            // và các headers khác có thể thay đổi giữa các request
            var userAgent = NormalizeHeader(context.Request.Headers.UserAgent.ToString());
            
            // Nếu User-Agent rỗng, fallback về IP
            if (string.IsNullOrEmpty(userAgent))
            {
                var ip = GetClientIpAddress(context);
                userAgent = ip;
            }

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(userAgent));
            
            return Convert.ToBase64String(hash);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate client fingerprint, using fallback");
            return "fallback_fingerprint";
        }
    }

    /// <summary>
    /// Normalize header value để tránh sự khác biệt về whitespace, case
    /// </summary>
    private static string NormalizeHeader(string headerValue)
    {
        if (string.IsNullOrEmpty(headerValue))
            return string.Empty;

        // Remove extra whitespace và convert to lowercase
        return headerValue.Trim().ToLowerInvariant();
    }

    public bool ValidateFingerprint(string storedFingerprint, HttpContext context)
    {
        try
        {
            if (string.IsNullOrEmpty(storedFingerprint))
                return true; // Backward compatibility

            var currentFingerprint = GenerateFingerprint(context);
            
            // Log để debug nếu fingerprint khác nhau
            if (storedFingerprint != currentFingerprint)
            {
                _logger.LogWarning(
                    "Fingerprint mismatch. Stored: {StoredFingerprint}, Current: {CurrentFingerprint}. " +
                    "IP: {Ip}, UserAgent: {UserAgent}",
                    storedFingerprint,
                    currentFingerprint,
                    GetClientIpAddress(context),
                    context.Request.Headers.UserAgent.ToString());
            }
            
            return storedFingerprint == currentFingerprint;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate client fingerprint, allowing request");
            return true; // Fail open for availability
        }
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP first (behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fallback to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}