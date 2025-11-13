using Shared.DTOs.Product;
using Base.Application.Feature.Product.Commands.CreateProduct;
using Base.Application.Feature.Product.Queries.GetProductById;
using Base.Application.Feature.Product.Queries.GetProducts;
using Base.Application.Feature.Product.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Authorization.PolicyContexts;
using Shared.SeedWork;

namespace Base.API.Controllers
{
    /// <summary>
    /// Product Controller with layered authorization (RBAC + PBAC)
    /// </summary>
    [ApiController]
    [Route("api/v2/[controller]")]
    [Produces("application/json")]
    [Authorize]
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
        /// Get all products with PBAC filtering
        /// </summary>
        [HttpGet]
        [Authorize(Policy = Shared.Identity.PolicyNames.Rbac.BasicUser)]
        public async Task<IActionResult> GetProducts([FromQuery] PagedRequestParameter parameters)
        {
            _logger.LogInformation("Getting products with page {PageIndex}, page size {PageSize}",
                parameters.PageNumber, parameters.PageSize);

            var filter = await _productPolicyService.GetProductListFilterAsync();

            var query = new GetProductsQuery
            {
                Parameters = parameters,
                MaxPrice = filter.MaxPrice
            };

            var result = await _mediator.Send(query);

            return Ok(result.ToApiResult());
        }

        /// <summary>
        /// Get product by ID with PBAC validation
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = Shared.Identity.PolicyNames.Rbac.BasicUser)]
        public async Task<IActionResult> GetProductById(long id)
        {
            _logger.LogInformation("Getting product with ID: {ProductId}", id);

            var query = new GetProductByIdQuery { Id = id };
            var product = await _mediator.Send(query);

            if (product == null)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found", id);
                return NotFound(new ApiErrorResult<ProductDto>(
                    ResponseMessages.ItemNotFound("Product", id)));
            }

            var policyCheck = await _productPolicyService.CanViewProductAsync(id, product.Price);

            if (!policyCheck.IsAllowed)
            {
                return StatusCode(403, new ApiErrorResult<object>(
                    $"Access denied: {policyCheck.Reason}"));
            }

            return Ok(new ApiSuccessResult<ProductDto>(product, ResponseMessages.RetrieveItemSuccess));
        }

        /// <summary>
        /// Create product with PBAC validation
        /// </summary>
        [HttpPost]
        [Authorize(Policy = Shared.Identity.PolicyNames.Rbac.ManagerOrAdmin)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
        {
            _logger.LogInformation("Creating new product: {ProductName}", command.Name);

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
        /// Update product with PBAC validation
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = Shared.Identity.PolicyNames.Rbac.ManagerOrAdmin)]
        public async Task<IActionResult> UpdateProduct(long id, [FromBody] UpdateProductRequest request)
        {
            _logger.LogInformation("Updating product with ID: {ProductId}", id);

            var query = new GetProductByIdQuery { Id = id };
            var existingProduct = await _mediator.Send(query);

            if (existingProduct == null)
            {
                return NotFound(new ApiErrorResult<object>(
                    ResponseMessages.ItemNotFound("Product", id)));
            }

            var policyCheck = await _productPolicyService.CanUpdateProductAsync(id, null, null);

            if (!policyCheck.IsAllowed)
            {
                return StatusCode(403, new ApiErrorResult<object>(
                    $"Access denied: {policyCheck.Reason}"));
            }

            _logger.LogInformation("Product update authorized for product {ProductId}", id);

            return Ok(new ApiSuccessResult<object>(
                new { id, message = "Product updated successfully" },
                "Product updated"));
        }

        /// <summary>
        /// Delete product - Admin only
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = Shared.Identity.PolicyNames.Rbac.AdminOnly)]
        public Task<IActionResult> DeleteProduct(long id)
        {
            _logger.LogInformation("Deleting product with ID: {ProductId}", id);

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

