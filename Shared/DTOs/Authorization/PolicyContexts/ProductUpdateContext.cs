namespace Shared.DTOs.Authorization.PolicyContexts
{
    /// <summary>
    /// Context for product update policy evaluation
    /// Contains information about the product being updated and its ownership
    /// </summary>
    public class ProductUpdateContext
    {
        public long ProductId { get; set; }
        public string? CreatedBy { get; set; }
        public string? ProductCategory { get; set; }
    }
}

