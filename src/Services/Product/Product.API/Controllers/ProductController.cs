using Asp.Versioning;
using Product.Application.Features.Products.Commands.CreateProduct;
using Product.Application.Features.Products.Commands.UpdateProduct;
using Product.Application.Features.Products.Commands.DeleteProduct;
using Product.Application.Features.Products.Queries.GetProductById;
using Product.Application.Features.Products.Queries.GetProducts;
using Shared.DTOs.Product;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.SeedWork;

namespace Product.API.Controllers
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
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="searchTerm">Search term</param>
        /// <param name="categoryId">Category ID</param>
        /// <param name="status">Product status</param>
        /// <returns>List of products</returns>
        /// <response code="200">Returns the list of products</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiSuccessResult<PagedResult<ProductDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] long? categoryId = null,
            [FromQuery] int? status = null)
        {
            _logger.LogInformation("Getting products with filters: page={Page}, pageSize={PageSize}, searchTerm={SearchTerm}, categoryId={CategoryId}, status={Status}", 
                page, pageSize, searchTerm, categoryId, status);
            
            var query = new GetProductsQuery
            {
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                Status = status
            };
            
            var result = await _mediator.Send(query);
            return Ok(new ApiSuccessResult<PagedResult<ProductDto>>(result, ResponseMessages.RetrieveItemsSuccess));
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
        [ProducesResponseType(typeof(ApiSuccessResult<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<ProductDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(long id)
        {
            _logger.LogInformation("Getting product with ID: {ProductId}", id);
            var query = new GetProductByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found", id);
                return NotFound(new ApiErrorResult<ProductDto>(
                    ResponseMessages.ItemNotFound("Product", id)));
            }

            return Ok(new ApiSuccessResult<ProductDto>(result, ResponseMessages.RetrieveItemSuccess));
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="dto">Product creation data</param>
        /// <returns>Created product details</returns>
        /// <response code="201">Product created successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiSuccessResult<ProductDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
        {
            _logger.LogInformation("Creating new product: {ProductName}", command.Name);

            var result = await _mediator.Send(command);
            _logger.LogInformation("Product created successfully with ID: {ProductId}", result.Id);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                new ApiSuccessResult<ProductDto>(result, ResponseMessages.ItemCreated("Product"))
            );
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="command">Product update data</param>
        /// <returns>Updated product details</returns>
        /// <response code="200">Product updated successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="404">Product not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<ProductDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResult<ProductDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateProductCommand command)
        {
            _logger.LogInformation("Updating product with ID: {ProductId}", id);

            if (id != command.Id)
            {
                _logger.LogWarning("ID mismatch - URL ID: {UrlId}, Body ID: {BodyId}", id, command.Id);
                return BadRequest(new ApiErrorResult<ProductDto>("ID in URL does not match ID in body"));
            }

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Product with ID: {ProductId} updated successfully", id);
                return Ok(new ApiSuccessResult<ProductDto>(result, ResponseMessages.ItemUpdated("Product")));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found for update", id);
                return NotFound(new ApiErrorResult<ProductDto>(
                    ResponseMessages.ItemNotFound("Product", id)));
            }
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

            try
            {
                var command = new DeleteProductCommand(id);
                var result = await _mediator.Send(command);
                _logger.LogInformation("Product with ID: {ProductId} deleted successfully", id);
                return Ok(new ApiSuccessResult<bool>(result, ResponseMessages.ItemDeleted("Product")));
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found for deletion", id);
                return NotFound(new ApiErrorResult<bool>(
                    ResponseMessages.ItemNotFound("Product", id)));
            }
        }
    }
}