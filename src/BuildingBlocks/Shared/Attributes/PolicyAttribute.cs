namespace Shared.Attributes
{
    /// <summary>
    /// Attribute to mark a class as a policy and define its policy name
    /// Use this on policy classes to enable auto-discovery
    /// </summary>
    /// <example>
    /// [Policy("PRODUCT:VIEW")]
    /// public class ProductViewPolicy : BasePolicy { }
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PolicyAttribute : Attribute
    {
        /// <summary>
        /// The policy name that will be used for registration and evaluation
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Optional description of the policy
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Creates a new Policy attribute
        /// </summary>
        /// <param name="name">The policy name (e.g., "PRODUCT:VIEW")</param>
        /// <exception cref="ArgumentException">Thrown when name is null or empty</exception>
        public PolicyAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Policy name cannot be empty", nameof(name));
            }

            Name = name;
        }
    }
}

