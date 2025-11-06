using Shared.Configurations.Database;

namespace ApiGateway.Configurations;

/// <summary>
/// BFF-specific settings kế thừa từ CacheSettings có sẵn
/// Thêm các config riêng cho BFF pattern như session expiration, PKCE expiration
/// </summary>
public class BffSettings : CacheSettings
{
    public const string SectionName = "BFF";

    /// <summary>
    /// Instance name - prefix cho tất cả keys trong Redis
    /// VD: BFF_session:abc123, BFF_pkce:xyz789
    /// </summary>
    public string InstanceName { get; set; } = "BFF_";

    /// <summary>
    /// Thời gian session tồn tại - Sliding expiration (phút)
    /// Session được extend nếu user active
    /// Default: 60 phút (1 giờ)
    /// </summary>
    public int SessionSlidingExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Thời gian session tồn tại - Absolute expiration (phút)
    /// Sau thời gian này phải login lại dù có active
    /// Default: 480 phút (8 giờ)
    /// </summary>
    public int SessionAbsoluteExpirationMinutes { get; set; } = 480;

    /// <summary>
    /// Thời gian PKCE code verifier tồn tại (phút)
    /// Default: 10 phút (đủ time để user login)
    /// </summary>
    public int PkceExpirationMinutes { get; set; } = 10;

    /// <summary>
    /// Số giây trước khi access token expire để trigger refresh
    /// VD: 60 = refresh token trước 60s khi sắp hết hạn
    /// </summary>
    public int RefreshTokenBeforeExpirationSeconds { get; set; } = 60;
}

