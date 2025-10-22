using Base.Application.Common.Models;
using Base.Application.Feature.Product.Commands.CreateProduct;
using Base.Application.Feature.Product.Queries.GetProductById;
using Base.Application.Feature.Product.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.SeedWork;

namespace Base.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IMediator mediator, ILogger<ProductController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] PagedRequestParameter parameters)
        {
            _logger.LogInformation("Getting products with page {PageIndex}, page size {PageSize}",
                parameters.PageNumber, parameters.PageSize);
            var query = new GetProductsQuery { Parameters = parameters };
            var result = await _mediator.Send(query);

            // Using the extension method to convert PagedResult to ApiResult with metadata
            return Ok(result.ToApiResult());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(long id)
        {
            _logger.LogInformation("Getting product with ID: {ProductId}", id);
            var query = new GetProductByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found", id);
                return NotFound(new ApiErrorResult<ProductDto>(
                    ResponseMessages.ItemNotFound("Product", id)));
            }

            return Ok(new ApiSuccessResult<ProductDto>(result, ResponseMessages.RetrieveItemSuccess));
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
        {
            _logger.LogInformation("Creating new product: {ProductName}", command.Name);
            var result = await _mediator.Send(command);
            _logger.LogInformation("Product created successfully with ID: {ProductId}", result);
            return CreatedAtAction(
                nameof(GetProductById),
                new { id = result },
                new ApiSuccessResult<long>(result, ResponseMessages.ItemCreated("Product")));
        }
    }
}

