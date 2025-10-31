namespace Shared.Configurations
{
    /// <summary>
    /// Keycloak configuration settings
    /// </summary>
    public class KeycloakSettings
    {
        public const string SectionName = "Keycloak";
        
        /// <summary>
        /// Keycloak server URL (e.g., http://localhost:8080)
        /// </summary>
        public string Authority { get; set; } = string.Empty;
        
        /// <summary>
        /// Realm name
        /// </summary>
        public string Realm { get; set; } = string.Empty;
        
        /// <summary>
        /// Client ID for the application
        /// </summary>
        public string ClientId { get; set; } = string.Empty;
        
        /// <summary>
        /// Client Secret (for confidential clients)
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;
        
        /// <summary>
        /// Metadata address (auto-generated from Authority and Realm if not specified)
        /// </summary>
        public string MetadataAddress => 
            $"{Authority}/realms/{Realm}/.well-known/openid-configuration";
        
        /// <summary>
        /// Token validation parameters
        /// </summary>
        public bool ValidateIssuer { get; set; } = true;
        public bool ValidateAudience { get; set; } = true;
        public bool ValidateLifetime { get; set; } = true;
        public bool RequireHttpsMetadata { get; set; } = false; // Set to true in production
        
        /// <summary>
        /// Role claim type (default: realm_access.roles or resource_access)
        /// </summary>
        public string RoleClaimType { get; set; } = "realm_access.roles";
        
        /// <summary>
        /// Enable role mapping from resource access
        /// </summary>
        public bool UseResourceRoles { get; set; } = true;
    }
}

