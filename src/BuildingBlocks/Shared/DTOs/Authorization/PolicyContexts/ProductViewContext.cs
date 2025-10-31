namespace Shared.DTOs.Authorization.PolicyContexts
{
    /// <summary>
    /// Context for product view policy evaluation
    /// Contains all necessary information to determine if a user can view a specific product
    /// </summary>
    public class ProductViewContext
    {
        public long ProductId { get; set; }
        public decimal ProductPrice { get; set; }
        public string? ProductCategory { get; set; }
    }
}

