namespace Infrastructure.Authorization.Models
{
    /// <summary>
    /// Represents dynamic policy configuration that can be loaded from:
    /// 1. JWT Claims (highest priority)
    /// 2. Configuration file (medium priority)
    /// 3. Hardcoded defaults (lowest priority - fallback)
    /// </summary>
    public class PolicyConfiguration
    {
        /// <summary>
        /// Maximum price restriction for product viewing/listing
        /// Can be overridden by JWT claim "policy:max_price"
        /// </summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Minimum price restriction
        /// Can be overridden by JWT claim "policy:min_price"
        /// </summary>
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// Allowed categories for filtering
        /// Can be overridden by JWT claim "policy:allowed_categories" (comma-separated)
        /// </summary>
        public List<string>? AllowedCategories { get; set; }

        /// <summary>
        /// Approval limit for orders/transactions
        /// Can be overridden by JWT claim "policy:approval_limit"
        /// </summary>
        public decimal? ApprovalLimit { get; set; }

        /// <summary>
        /// Custom policy attributes from JWT
        /// Can contain any key-value pairs from claims
        /// </summary>
        public Dictionary<string, object>? CustomAttributes { get; set; }

        /// <summary>
        /// Creates an empty configuration (no restrictions)
        /// </summary>
        public static PolicyConfiguration Empty() => new();

        /// <summary>
        /// Merges this configuration with another, with the other taking priority
        /// </summary>
        public PolicyConfiguration MergeWith(PolicyConfiguration? other)
        {
            if (other == null) return this;

            return new PolicyConfiguration
            {
                MaxPrice = other.MaxPrice ?? this.MaxPrice,
                MinPrice = other.MinPrice ?? this.MinPrice,
                AllowedCategories = other.AllowedCategories ?? this.AllowedCategories,
                ApprovalLimit = other.ApprovalLimit ?? this.ApprovalLimit,
                CustomAttributes = MergeDictionaries(this.CustomAttributes, other.CustomAttributes)
            };
        }

        private static Dictionary<string, object>? MergeDictionaries(
            Dictionary<string, object>? dict1,
            Dictionary<string, object>? dict2)
        {
            if (dict1 == null) return dict2;
            if (dict2 == null) return dict1;

            var merged = new Dictionary<string, object>(dict1);
            foreach (var kvp in dict2)
            {
                merged[kvp.Key] = kvp.Value; // dict2 takes priority
            }
            return merged;
        }
    }
}

