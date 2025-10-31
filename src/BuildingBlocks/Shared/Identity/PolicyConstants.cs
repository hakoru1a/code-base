namespace Shared.Identity
{
    /// <summary>
    /// Centralized constants for policy evaluation
    /// Contains context keys, user attributes, and standard messages
    /// </summary>
    public static class PolicyConstants
    {
        /// <summary>
        /// Context keys for passing data to policy handlers
        /// Use these when building policy context dictionaries
        /// </summary>
        public static class ContextKeys
        {
            // Product-related context keys
            public const string ProductId = "ProductId";
            public const string ProductPrice = "ProductPrice";
            public const string ProductCategory = "ProductCategory";
            public const string CreatedBy = "CreatedBy";
            public const string MaxPrice = "MaxPrice";
            public const string AllowedCategories = "AllowedCategories";

            // Order-related context keys
            public const string OrderId = "OrderId";
            public const string OrderTotal = "OrderTotal";
            public const string OrderStatus = "OrderStatus";

            // User-related context keys
            public const string UserId = "UserId";
            public const string UserRole = "UserRole";
        }

        /// <summary>
        /// User custom attribute keys from JWT claims or user profile
        /// </summary>
        public static class UserAttributes
        {
            public const string Department = "department";
            public const string Region = "region";
            public const string CostCenter = "cost_center";
            public const string Team = "team";
        }

        /// <summary>
        /// Standard policy evaluation messages
        /// Provides consistent messaging across all policies
        /// </summary>
        public static class Messages
        {
            // General allow messages
            public const string AdminFullAccess = "Admin has full access";
            public const string PremiumUserFullAccess = "Premium user has full access";
            public const string UserHasRequiredRoleAndPermission = "User has required role and permission";

            // Product-specific allow messages
            public const string UserCanUpdateOwnProducts = "User can update their own products";
            public const string ProductManagerCanUpdateInCategory = "Product manager can update products in their category";

            // General deny messages
            public const string UserDoesNotHavePermissionToUpdate = "User does not have permission to update this product";
            public const string UserDoesNotHaveRequiredRole = "User does not have required role";
            public const string UserDoesNotHaveRoleToViewProducts = "User does not have required role to view products";

            // Product-specific deny messages
            public const string UserHasRoleButMissingProductCreatePermission = "User has required role but missing 'product:create' permission";
            public const string UserDoesNotHaveRequiredRoleForProductCreate = "User does not have required role (admin, manager, or product_manager)";

            // Message templates (use with string.Format)
            public const string PriceExceedsLimitTemplate = "Product price {0:N0} VND exceeds {1} limit of {2:N0} VND";
            public const string PriceWithinLimitTemplate = "Product price {0:N0} VND is within limit of {1:N0} VND";
            public const string NoRestrictionTemplate = "{0} has no price restrictions (no configuration found)";
            public const string FilterAppliedTemplate = "Filter applied for user role: {0}";

            // Additional templates
            public const string PriceExceedsConfiguredLimitTemplate = "Product price {0:N0} VND exceeds the configured limit of {1:N0} VND for {2}";
            public const string UserDeniedAccessTemplate = "User {0} denied access to {1} {2}. Reason: {3}";
        }

        /// <summary>
        /// Configuration source identifiers
        /// Used to identify where policy configuration comes from
        /// </summary>
        public static class ConfigurationSources
        {
            public const string JwtClaims = "JWT claims";
            public const string RoleConfiguration = "role configuration";
            public const string DefaultConfiguration = "default configuration";
            public const string AppSettings = "appsettings.json";
        }
    }
}

