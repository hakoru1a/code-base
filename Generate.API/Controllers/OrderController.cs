using Asp.Versioning;
using Generate.Application.Common.DTOs.Order;
using Generate.Application.Features.Order.Commands.CreateOrder;
using Generate.Application.Features.Order.Commands.DeleteOrder;
using Generate.Application.Features.Order.Commands.UpdateOrder;
using Generate.Application.Features.Order.Queries.GetOrderById;
using Generate.Application.Features.Order.Queries.GetOrders;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.SeedWork;

namespace Generate.API.Controllers
{
    /// <summary>
    /// Controller for managing orders
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IMediator mediator, ILogger<OrderController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all orders
        /// </summary>
        /// <returns>List of orders</returns>
        /// <response code="200">Returns the list of orders</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiSuccessResult<List<OrderResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetList()
        {
            _logger.LogInformation("Getting all orders");
            var query = new GetOrdersQuery();
            var result = await _mediator.Send(query);
            return Ok(new ApiSuccessResult<List<OrderResponseDto>>(result, ResponseMessages.RetrieveItemsSuccess));
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <returns>Order details</returns>
        /// <response code="200">Returns the order</response>
        /// <response code="404">Order not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<OrderResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(long id)
        {
            _logger.LogInformation("Getting order with ID: {OrderId}", id);
            var query = new GetOrderByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                _logger.LogWarning("Order with ID: {OrderId} not found", id);
                return NotFound(new ApiErrorResult<OrderResponseDto>(
                    ResponseMessages.ItemNotFound("Order", id)));
            }

            return Ok(new ApiSuccessResult<OrderResponseDto>(result, ResponseMessages.RetrieveItemSuccess));
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        /// <param name="dto">Order creation data</param>
        /// <returns>ID of the created order</returns>
        /// <response code="201">Order created successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiSuccessResult<long>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] OrderCreateDto dto)
        {
            _logger.LogInformation("Creating new order for customer: {CustomerName}", dto.CustomerName);

            var command = new CreateOrderCommand
            {
                CustomerName = dto.CustomerName,
                OrderItems = dto.OrderItems
            };

            var orderId = await _mediator.Send(command);
            _logger.LogInformation("Order created successfully with ID: {OrderId}", orderId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = orderId },
                new ApiSuccessResult<long>(orderId, ResponseMessages.ItemCreated("Order"))
            );
        }

        /// <summary>
        /// Update an existing order
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="dto">Order update data</param>
        /// <returns>Update result</returns>
        /// <response code="200">Order updated successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="404">Order not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(long id, [FromBody] OrderUpdateDto dto)
        {
            _logger.LogInformation("Updating order with ID: {OrderId}", id);

            if (id != dto.Id)
            {
                _logger.LogWarning("ID mismatch - URL ID: {UrlId}, Body ID: {BodyId}", id, dto.Id);
                return BadRequest(new ApiErrorResult<bool>("ID in URL does not match ID in body"));
            }

            var command = new UpdateOrderCommand
            {
                Id = dto.Id,
                CustomerName = dto.CustomerName,
                OrderItems = dto.OrderItems
            };

            var result = await _mediator.Send(command);

            if (!result)
            {
                _logger.LogWarning("Order with ID: {OrderId} not found for update", id);
                return NotFound(new ApiErrorResult<bool>(
                    ResponseMessages.ItemNotFound("Order", id)));
            }

            _logger.LogInformation("Order with ID: {OrderId} updated successfully", id);
            return Ok(new ApiSuccessResult<bool>(result, ResponseMessages.ItemUpdated("Order")));
        }

        /// <summary>
        /// Delete an order
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <returns>Delete result</returns>
        /// <response code="200">Order deleted successfully</response>
        /// <response code="404">Order not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id)
        {
            _logger.LogInformation("Deleting order with ID: {OrderId}", id);

            var command = new DeleteOrderCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result)
            {
                _logger.LogWarning("Order with ID: {OrderId} not found for deletion", id);
                return NotFound(new ApiErrorResult<bool>(
                    ResponseMessages.ItemNotFound("Order", id)));
            }

            _logger.LogInformation("Order with ID: {OrderId} deleted successfully", id);
            return Ok(new ApiSuccessResult<bool>(result, ResponseMessages.ItemDeleted("Order")));
        }
    }
}

