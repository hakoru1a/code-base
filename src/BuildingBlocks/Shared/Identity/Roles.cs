namespace Shared.Identity
{
    /// <summary>
    /// Centralized role constants for authorization across the application
    /// Use these constants instead of magic strings for compile-time safety
    /// </summary>
    public static class Roles
    {
        /// <summary>
        /// Administrator with full system access
        /// </summary>
        public const string Admin = "admin";

        /// <summary>
        /// General manager role
        /// </summary>
        public const string Manager = "manager";

        /// <summary>
        /// Product manager with category-based access
        /// </summary>
        public const string ProductManager = "product_manager";

        /// <summary>
        /// Premium user with extended privileges
        /// </summary>
        public const string PremiumUser = "premium_user";

        /// <summary>
        /// Basic user with limited access
        /// </summary>
        public const string BasicUser = "basic_user";
    }
}
