using Asp.Versioning;
using Shared.DTOs.Product;
using Generate.Application.Features.Product.Commands.CreateProduct;
using Generate.Application.Features.Product.Commands.DeleteProduct;
using Generate.Application.Features.Product.Commands.UpdateProduct;
using Generate.Application.Features.Product.Queries.GetProductById;
using Generate.Application.Features.Product.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.SeedWork;

namespace Generate.API.Controllers
{
    /// <summary>
    /// Controller for managing products
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class ProductController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IMediator mediator, ILogger<ProductController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            _logger.LogInformation("Getting all products");
            var query = new GetProductsQuery();
            var result = await _mediator.Send(query);
            return Ok(new ApiSuccessResult<List<ProductResponseDto>>(result, ResponseMessages.RetrieveItemsSuccess));
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
            _logger.LogInformation("Getting product with ID: {ProductId}", id);
            var query = new GetProductByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found", id);
                return NotFound(new ApiErrorResult<ProductResponseDto>(
                    ResponseMessages.ItemNotFound("Product", id)));
            }

            return Ok(new ApiSuccessResult<ProductResponseDto>(result, ResponseMessages.RetrieveItemSuccess));
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
            _logger.LogInformation("Creating new product: {ProductName}", dto.Name);

            var command = new CreateProductCommand
            {
                Name = dto.Name,
                CategoryId = dto.CategoryId
            };

            var productId = await _mediator.Send(command);
            _logger.LogInformation("Product created successfully with ID: {ProductId}", productId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = productId },
                new ApiSuccessResult<long>(productId, ResponseMessages.ItemCreated("Product"))
            );
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
            _logger.LogInformation("Updating product with ID: {ProductId}", id);

            if (id != dto.Id)
            {
                _logger.LogWarning("ID mismatch - URL ID: {UrlId}, Body ID: {BodyId}", id, dto.Id);
                return BadRequest(new ApiErrorResult<bool>("ID in URL does not match ID in body"));
            }

            var command = new UpdateProductCommand
            {
                Id = dto.Id,
                Name = dto.Name,
                CategoryId = dto.CategoryId
            };

            var result = await _mediator.Send(command);

            if (!result)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found for update", id);
                return NotFound(new ApiErrorResult<bool>(
                    ResponseMessages.ItemNotFound("Product", id)));
            }

            _logger.LogInformation("Product with ID: {ProductId} updated successfully", id);
            return Ok(new ApiSuccessResult<bool>(result, ResponseMessages.ItemUpdated("Product")));
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
            _logger.LogInformation("Deleting product with ID: {ProductId}", id);

            var command = new DeleteProductCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found for deletion", id);
                return NotFound(new ApiErrorResult<bool>(
                    ResponseMessages.ItemNotFound("Product", id)));
            }

            _logger.LogInformation("Product with ID: {ProductId} deleted successfully", id);
            return Ok(new ApiSuccessResult<bool>(result, ResponseMessages.ItemDeleted("Product")));
        }
    }
}
