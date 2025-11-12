namespace ApiGateway.Configurations;

/// <summary>
/// Public paths that don't require authentication
/// </summary>
public static class PublicPaths
{
    public static readonly HashSet<string> Paths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health",
        "/swagger",
        "/auth/login",
        "/auth/signin-oidc",
        "/auth/logout",
        "/auth/signout-callback-oidc"
    };
}

