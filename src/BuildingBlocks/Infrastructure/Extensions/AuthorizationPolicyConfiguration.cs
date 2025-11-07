using Microsoft.AspNetCore.Authorization;
using Shared.Identity;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Centralized authorization policy configuration
    /// This file contains all authorization policies for easy visual configuration
    /// </summary>
    public static class AuthorizationPolicyConfiguration
    {
        /// <summary>
        /// Configure all authorization policies
        /// This method is called from AddKeycloakAuthorization extension method
        /// </summary>
        public static void ConfigurePolicies(AuthorizationOptions options)
        {
            ConfigureRbacPolicies(options);
            ConfigureHybridPolicies(options);
        }

        /// <summary>
        /// Configure Role-Based Access Control (RBAC) policies
        /// These provide coarse-grained access control at the gateway/controller level
        /// </summary>
        private static void ConfigureRbacPolicies(AuthorizationOptions options)
        {
            // ===== ROLE-BASED POLICIES (RBAC) =====
            // These provide coarse-grained access control at the gateway/controller level

            // Admin Only: Requires admin role
            options.AddPolicy(
                PolicyNames.Rbac.AdminOnly,
                policy => policy.RequireRole(Roles.Admin));

            // Manager Or Admin: Requires admin, manager, or product_manager role
            options.AddPolicy(
                PolicyNames.Rbac.ManagerOrAdmin,
                policy => policy.RequireRole(
                    Roles.Admin,
                    Roles.Manager,
                    Roles.ProductManager));

            // Authenticated User: Requires any authenticated user
            options.AddPolicy(
                PolicyNames.Rbac.AuthenticatedUser,
                policy => policy.RequireAuthenticatedUser());

            // Premium User: Requires premium_user or admin role
            options.AddPolicy(
                PolicyNames.Rbac.PremiumUser,
                policy => policy.RequireRole(
                    Roles.PremiumUser,
                    Roles.Admin));

            // Basic User: Requires basic_user, premium_user, or admin role
            options.AddPolicy(
                PolicyNames.Rbac.BasicUser,
                policy => policy.RequireRole(
                    Roles.BasicUser,
                    Roles.PremiumUser,
                    Roles.Admin));
        }

        /// <summary>
        /// Configure Hybrid policies (Role + Permission)
        /// These combine roles and permissions for flexible access control
        /// 
        /// Logic: User can access if they have the required permission OR have one of the allowed roles
        /// This provides flexibility: fine-grained control via permissions, quick access via roles
        /// </summary>
        private static void ConfigureHybridPolicies(AuthorizationOptions options)
        {
            // ===== HYBRID POLICIES (Role + Permission) =====
            // These combine roles and permissions for flexible access control
            // Uses helper method for maintainability and consistency

            // Product Policies
            ConfigureProductPolicies(options);

            // Category Policies
            ConfigureCategoryPolicies(options);
        }

        /// <summary>
        /// Configure Product-related hybrid policies
        /// </summary>
        private static void ConfigureProductPolicies(AuthorizationOptions options)
        {
            // CanViewProducts: Permission "product:view" OR roles "admin"/"manager"
            KeycloakAuthenticationExtensions.AddHybridPolicy(
                options,
                PolicyNames.Hybrid.Product.CanView,
                Permissions.Product.View,
                Roles.Admin,
                Roles.Manager);

            // CanCreateProducts: Permission "product:create" OR roles "admin"/"product_manager"
            KeycloakAuthenticationExtensions.AddHybridPolicy(
                options,
                PolicyNames.Hybrid.Product.CanCreate,
                Permissions.Product.Create,
                Roles.Admin,
                Roles.ProductManager);

            // CanUpdateProducts: Permission "product:update" OR roles "admin"/"product_manager"
            KeycloakAuthenticationExtensions.AddHybridPolicy(
                options,
                PolicyNames.Hybrid.Product.CanUpdate,
                Permissions.Product.Update,
                Roles.Admin,
                Roles.ProductManager);

            // CanDeleteProducts: Permission "product:delete" OR role "admin" (more restrictive)
            KeycloakAuthenticationExtensions.AddHybridPolicy(
                options,
                PolicyNames.Hybrid.Product.CanDelete,
                Permissions.Product.Delete,
                Roles.Admin);
        }

        /// <summary>
        /// Configure Category-related hybrid policies
        /// </summary>
        private static void ConfigureCategoryPolicies(AuthorizationOptions options)
        {
            // CanViewCategories: Permission "category:view" OR roles "admin"/"manager"
            KeycloakAuthenticationExtensions.AddHybridPolicy(
                options,
                PolicyNames.Hybrid.Category.CanView,
                Permissions.Category.View,
                Roles.Admin,
                Roles.Manager);
        }
    }
}

