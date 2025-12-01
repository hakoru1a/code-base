using Asp.Versioning;
using Shared.DTOs.Product;
using Generate.Application.Features.Products.Commands.CreateProduct;
using Generate.Application.Features.Products.Commands.DeleteProduct;
using Generate.Application.Features.Products.Commands.UpdateProduct;
using Generate.Application.Features.Products.Queries.GetProductById;
using Generate.Application.Features.Products.Queries.GetProducts;
using Infrastructure.Common;
using Infrastructure.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Attributes;
using Shared.SeedWork;

namespace Generate.API.Controllers
{
    /// <summary>
    /// Controller for managing products
    /// </summary>
    [ApiVersion("1.0")]
    public class ProductController : ApiControllerBase<ProductController>
    {
        private const string EntityName = "Product";

        public ProductController(IMediator mediator, ILogger<ProductController> logger)
            : base(mediator, logger)
        {
        }

        /// <summary>
        /// Get all products with dynamic filtering based on user permissions and JWT claims
        /// Policy PRODUCT:VIEW applies filtering based on:
        /// - max_product_price from JWT claim (e.g., 20,000,000 VND)
        /// - department from JWT claim for department-specific products
        /// - region from JWT claim for region-specific products  
        /// - category permissions for category restrictions
        /// </summary>
        /// <returns>List of products filtered based on user context</returns>
        /// <response code="200">Returns the filtered list of products</response>
        /// <response code="403">Forbidden - User lacks required permissions</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [RequirePolicy("PRODUCT:VIEW")]
        [ProducesResponseType(typeof(ApiSuccessResult<List<ProductResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetList()
        {
            var query = new GetProductsQuery();

            // Policy filter context is automatically applied in the query handler
            // via HttpContext.GetProductFilterContext()
            var result = await HandleGetAllAsync<GetProductsQuery, ProductResponseDto>(query, EntityName);

            // Log applied filters for debugging (optional)
            var filterContext = HttpContext.GetProductFilterContext();
            if (filterContext != null)
            {
                Logger.LogInformation(
                    "Applied product filters for user - MaxPrice: {MaxPrice}, CanViewAll: {CanViewAll}, " +
                    "Department: {Department}, AllowedCategories: [{Categories}]",
                    filterContext.MaxPrice,
                    filterContext.CanViewAll,
                    filterContext.DepartmentFilter,
                    string.Join(", ", filterContext.AllowedCategories ?? new List<string>()));
            }

            return result;
        }

        /// <summary>
        /// Get product by ID - requires PRODUCT:VIEW policy
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product details</returns>
        /// <response code="200">Returns the product</response>
        /// <response code="403">Forbidden - User lacks required permissions</response>
        /// <response code="404">Product not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [RequirePolicy("PRODUCT:VIEW")]
        [ProducesResponseType(typeof(ApiSuccessResult<ProductResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResult<ProductResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(long id)
        {
            var query = new GetProductByIdQuery { Id = id };
            return await HandleGetByIdAsync<GetProductByIdQuery, ProductResponseDto>(query, id, EntityName);
        }

        /// <summary>
        /// Create a new product - requires PRODUCT:CREATE policy
        /// </summary>
        /// <param name="dto">Product creation data</param>
        /// <returns>ID of the created product</returns>
        /// <response code="201">Product created successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="403">Forbidden - User lacks required permissions</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [RequirePolicy("PRODUCT:CREATE")]
        [ProducesResponseType(typeof(ApiSuccessResult<long>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
        {
            var command = new CreateProductCommand
            {
                Name = dto.Name,
                CategoryId = dto.CategoryId
            };

            return await HandleCreateAsync(command, EntityName, dto.Name);
        }

        /// <summary>
        /// Update an existing product - requires PRODUCT:UPDATE policy
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="dto">Product update data</param>
        /// <returns>Update result</returns>
        /// <response code="200">Product updated successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="403">Forbidden - User lacks required permissions</response>
        /// <response code="404">Product not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        [RequirePolicy("PRODUCT:UPDATE")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(long id, [FromBody] ProductUpdateDto dto)
        {
            var command = new UpdateProductCommand
            {
                Id = dto.Id,
                Name = dto.Name,
                CategoryId = dto.CategoryId
            };

            return await HandleUpdateAsync(command, id, dto.Id, EntityName);
        }

        /// <summary>
        /// Delete a product - requires PRODUCT:DELETE policy
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Delete result</returns>
        /// <response code="200">Product deleted successfully</response>
        /// <response code="403">Forbidden - User lacks required permissions</response>
        /// <response code="404">Product not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [RequirePolicy("PRODUCT:DELETE")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id)
        {
            var command = new DeleteProductCommand { Id = id };
            return await HandleDeleteAsync(command, id, EntityName);
        }
    }
}
