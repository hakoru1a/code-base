using Asp.Versioning;
using Shared.DTOs.Product;
using Generate.Application.Features.Product.Commands.CreateProduct;
using Generate.Application.Features.Product.Commands.DeleteProduct;
using Generate.Application.Features.Product.Commands.UpdateProduct;
using Generate.Application.Features.Product.Queries.GetProductById;
using Generate.Application.Features.Product.Queries.GetProducts;
using Infrastructure.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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
        /// Get all products
        /// </summary>
        /// <returns>List of products</returns>
        /// <response code="200">Returns the list of products</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiSuccessResult<List<ProductResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetList()
        {
            var query = new GetProductsQuery();
            return await HandleGetAllAsync<GetProductsQuery, ProductResponseDto>(query, EntityName);
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product details</returns>
        /// <response code="200">Returns the product</response>
        /// <response code="404">Product not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<ProductResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<ProductResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(long id)
        {
            var query = new GetProductByIdQuery { Id = id };
            return await HandleGetByIdAsync<GetProductByIdQuery, ProductResponseDto>(query, id, EntityName);
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="dto">Product creation data</param>
        /// <returns>ID of the created product</returns>
        /// <response code="201">Product created successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiSuccessResult<long>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        /// Update an existing product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="dto">Product update data</param>
        /// <returns>Update result</returns>
        /// <response code="200">Product updated successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="404">Product not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status400BadRequest)]
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
        /// Delete a product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Delete result</returns>
        /// <response code="200">Product deleted successfully</response>
        /// <response code="404">Product not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id)
        {
            var command = new DeleteProductCommand { Id = id };
            return await HandleDeleteAsync(command, id, EntityName);
        }
    }
}
