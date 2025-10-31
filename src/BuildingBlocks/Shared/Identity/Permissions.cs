namespace Shared.Identity
{
    /// <summary>
    /// Centralized permission constants for fine-grained access control
    /// Use these constants instead of magic strings for compile-time safety
    /// Format: {resource}:{action}[:{scope}]
    /// </summary>
    public static class Permissions
    {
        /// <summary>
        /// Product-related permissions
        /// </summary>
        public static class Product
        {
            public const string Create = "product:create";
            public const string View = "product:view";
            public const string Update = "product:update";
            public const string UpdateOwn = "product:update:own";
            public const string Delete = "product:delete";
            public const string DeleteOwn = "product:delete:own";
            public const string Approve = "product:approve";
        }

        /// <summary>
        /// Order-related permissions
        /// </summary>
        public static class Order
        {
            public const string Create = "order:create";
            public const string View = "order:view";
            public const string ViewOwn = "order:view:own";
            public const string Update = "order:update";
            public const string Cancel = "order:cancel";
            public const string Approve = "order:approve";
        }

        /// <summary>
        /// User management permissions
        /// </summary>
        public static class User
        {
            public const string Create = "user:create";
            public const string View = "user:view";
            public const string Update = "user:update";
            public const string Delete = "user:delete";
            public const string ManageRoles = "user:manage_roles";
        }
    }
}
