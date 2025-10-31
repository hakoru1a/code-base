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
        /// PBAC Policies - Policy-based authorization (Service level)
        /// These are used with [RequirePolicy("...")] attribute or IProductPolicyService
        /// </summary>
        public static class Pbac
        {
            /// <summary>
            /// Product policies
            /// </summary>
            public static class Product
            {
                public const string View = "PRODUCT:VIEW";
                public const string Create = "PRODUCT:CREATE";
                public const string Update = "PRODUCT:UPDATE";
                public const string Delete = "PRODUCT:DELETE";
                public const string ListFilter = "PRODUCT:LIST_FILTER";
            }

            /// <summary>
            /// Order policies
            /// </summary>
            public static class Order
            {
                public const string View = "ORDER:VIEW";
                public const string Create = "ORDER:CREATE";
                public const string Update = "ORDER:UPDATE";
                public const string Cancel = "ORDER:CANCEL";
                public const string Approve = "ORDER:APPROVE";
            }
        }
    }
}

