namespace Shared.Identity
{
    /// <summary>
    /// Centralized policy names for RBAC authorization
    /// Use these constants for ASP.NET Core's built-in authorization policies
    /// </summary>
    public static class PolicyNames
    {
        /// <summary>
        /// RBAC Policies - Role-based authorization (Gateway level)
        /// These are used with [Authorize(Policy = "...")] attribute
        /// </summary>
        public static class Rbac
        {
            /// <summary>
            /// Requires admin role only
            /// </summary>
            public const string AdminOnly = "AdminOnly";

            /// <summary>
            /// Requires admin, manager, or product_manager role
            /// </summary>
            public const string ManagerOrAdmin = "ManagerOrAdmin";

            /// <summary>
            /// Requires any authenticated user
            /// </summary>
            public const string AuthenticatedUser = "AuthenticatedUser";

            /// <summary>
            /// Requires premium_user or admin role
            /// </summary>
            public const string PremiumUser = "PremiumUser";

            /// <summary>
            /// Requires basic_user, premium_user, or admin role
            /// </summary>
            public const string BasicUser = "BasicUser";
        }


        /// <summary>
        /// Hybrid Policies - Combine roles and permissions (Gateway level)
        /// These provide flexible access control combining RBAC and PBAC
        /// Use these with [Authorize(Policy = "...")] attribute
        /// </summary>
        public static class Hybrid
        {
            /// <summary>
            /// Product-related hybrid policies
            /// </summary>
            public static class Product
            {
                public const string CanView = "CanViewProducts";
                public const string CanCreate = "CanCreateProducts";
                public const string CanUpdate = "CanUpdateProducts";
                public const string CanDelete = "CanDeleteProducts";
            }

            /// <summary>
            /// Category-related hybrid policies
            /// </summary>
            public static class Category
            {
                public const string CanView = "CanViewCategories";
                public const string CanCreate = "CanCreateCategories";
                public const string CanUpdate = "CanUpdateCategories";
                public const string CanDelete = "CanDeleteCategories";
            }
        }
    }
}

