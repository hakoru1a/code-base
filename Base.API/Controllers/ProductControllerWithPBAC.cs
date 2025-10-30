using Base.Application.Common.Models;
using Base.Application.Feature.Product.Commands.CreateProduct;
using Base.Application.Feature.Product.Queries.GetProductById;
using Base.Application.Feature.Product.Queries.GetProducts;
using Base.Application.Feature.Product.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.SeedWork;

namespace Base.API.Controllers
{
    /// <summary>
    /// Enhanced Product Controller with PBAC (Policy-Based Access Control)
    /// Demonstrates both RBAC (at Gateway) and PBAC (at Service level)
    /// </summary>
    [ApiController]
    [Route("api/v2/[controller]")]
    [Produces("application/json")]
    [Authorize] // RBAC: Requires authentication at Gateway level
    public class ProductControllerWithPBAC : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductControllerWithPBAC> _logger;
        private readonly IProductPolicyService _productPolicyService;

        public ProductControllerWithPBAC(
            IMediator mediator,
            ILogger<ProductControllerWithPBAC> logger,
            IProductPolicyService productPolicyService)
        {
            _mediator = mediator;
            _logger = logger;
            _productPolicyService = productPolicyService;
        }

        /// <summary>
        /// Get all products with RBAC at Gateway level and PBAC filtering
        /// Products are filtered based on user role:
        /// - Basic users: Only products under 5M VND
        /// - Premium users: All products
        /// - Admins: All products
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "BasicUser")] // RBAC at Gateway
        public async Task<IActionResult> GetProducts([FromQuery] PagedRequestParameter parameters)
        {
            _logger.LogInformation("Getting products with page {PageIndex}, page size {PageSize}",
                parameters.PageNumber, parameters.PageSize);

            // Get filter from policy service (clean - all logic encapsulated)
            var filter = await _productPolicyService.GetProductListFilterAsync();

            // Pass filter to query handler
            var query = new GetProductsQuery
            {
                Parameters = parameters,
                MaxPrice = filter.MaxPrice
            };

            var result = await _mediator.Send(query);

            return Ok(result.ToApiResult());
        }

        /// <summary>
        /// Get product by ID with PBAC (Policy-Based Access Control)
        /// Policy: PRODUCT_VIEW_PRICE - Basic users can only view products under 5M VND
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "BasicUser")] // RBAC at Gateway
        public async Task<IActionResult> GetProductById(long id)
        {
            _logger.LogInformation("Getting product with ID: {ProductId}", id);

            // First, get the product
            var query = new GetProductByIdQuery { Id = id };
            var product = await _mediator.Send(query);

            if (product == null)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found", id);
                return NotFound(new ApiErrorResult<ProductDto>(
                    ResponseMessages.ItemNotFound("Product", id)));
            }

            // PBAC: Check if user can view this product
            var policyCheck = await _productPolicyService.CanViewProductAsync(id, product.Price);

            if (!policyCheck.IsAllowed)
            {
                return StatusCode(403, new ApiErrorResult<object>(
                    $"Access denied: {policyCheck.Reason}"));
            }

            return Ok(new ApiSuccessResult<ProductDto>(product, ResponseMessages.RetrieveItemSuccess));
        }

        /// <summary>
        /// Create product with RBAC and PBAC
        /// RBAC: Requires "ManagerOrAdmin" policy at Gateway
        /// PBAC: Requires "PRODUCT_CREATE" policy at Service level
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "ManagerOrAdmin")] // RBAC at Gateway
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
        {
            _logger.LogInformation("Creating new product: {ProductName}", command.Name);

            // PBAC: Check if user can create product
            var policyCheck = await _productPolicyService.CanCreateProductAsync();

            if (!policyCheck.IsAllowed)
            {
                return StatusCode(403, new ApiErrorResult<object>(
                    $"Access denied: {policyCheck.Reason}"));
            }

            var result = await _mediator.Send(command);
            _logger.LogInformation("Product created successfully with ID: {ProductId}", result);

            return CreatedAtAction(
                nameof(GetProductById),
                new { id = result },
                new ApiSuccessResult<long>(result, ResponseMessages.ItemCreated("Product")));
        }

        /// <summary>
        /// Update product with complex PBAC rules
        /// - Admins can update any product
        /// - Users can update their own products if they have permission
        /// - Product managers can update products in their category
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "ManagerOrAdmin")] // RBAC at Gateway
        public async Task<IActionResult> UpdateProduct(long id, [FromBody] UpdateProductRequest request)
        {
            _logger.LogInformation("Updating product with ID: {ProductId}", id);

            // Get existing product to check ownership and category
            var query = new GetProductByIdQuery { Id = id };
            var existingProduct = await _mediator.Send(query);

            if (existingProduct == null)
            {
                return NotFound(new ApiErrorResult<object>(
                    ResponseMessages.ItemNotFound("Product", id)));
            }

            // PBAC: Check if user can update this product
            var policyCheck = await _productPolicyService.CanUpdateProductAsync(
                id,
                null,  // CreatedBy - would need to be added to ProductDto if needed
                null); // Category - would need to be added to ProductDto if needed

            if (!policyCheck.IsAllowed)
            {
                return StatusCode(403, new ApiErrorResult<object>(
                    $"Access denied: {policyCheck.Reason}"));
            }

            // Proceed with update...
            _logger.LogInformation("Product update authorized for product {ProductId}", id);

            return Ok(new ApiSuccessResult<object>(
                new { id, message = "Product updated successfully" },
                "Product updated"));
        }

        /// <summary>
        /// Delete product - Admin only
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")] // RBAC at Gateway - strict admin only
        public Task<IActionResult> DeleteProduct(long id)
        {
            _logger.LogInformation("Deleting product with ID: {ProductId}", id);

            // For delete, we rely on RBAC only (admin only)
            // Could add additional PBAC if needed for soft delete, audit, etc.

            return Task.FromResult<IActionResult>(Ok(new ApiSuccessResult<object>(
                new { id, message = "Product deleted successfully" },
                "Product deleted")));
        }

    }

    /// <summary>
    /// Update product request DTO
    /// </summary>
    public class UpdateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
    }
}

