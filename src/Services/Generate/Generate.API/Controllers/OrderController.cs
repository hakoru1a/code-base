using Asp.Versioning;
using Shared.DTOs.Order;
using Generate.Application.Features.Order.Commands.CreateOrder;
using Generate.Application.Features.Order.Commands.DeleteOrder;
using Generate.Application.Features.Order.Commands.UpdateOrder;
using Generate.Application.Features.Order.Queries.GetOrderById;
using Generate.Application.Features.Order.Queries.GetOrders;
using Infrastructure.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Attributes;
using Shared.SeedWork;

namespace Generate.API.Controllers
{
    /// <summary>
    /// Controller for managing orders
    /// </summary>
    [ApiVersion("1.0")]
    public class OrderController : ApiControllerBase<OrderController>
    {
        private const string EntityName = "Order";

        public OrderController(IMediator mediator, ILogger<OrderController> logger)
            : base(mediator, logger)
        {
        }

        /// <summary>
        /// Get all orders - requires ORDER:VIEW policy
        /// </summary>
        /// <returns>List of orders</returns>
        /// <response code="200">Returns the list of orders</response>
        /// <response code="403">Forbidden - User lacks required permissions</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [RequirePolicy("ORDER:VIEW")]
        [ProducesResponseType(typeof(ApiSuccessResult<List<OrderResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetList()
        {
            var query = new GetOrdersQuery();
            return await HandleGetAllAsync<GetOrdersQuery, OrderResponseDto>(query, EntityName);
        }

        /// <summary>
        /// Get order by ID - requires ORDER:VIEW policy
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <returns>Order details</returns>
        /// <response code="200">Returns the order</response>
        /// <response code="403">Forbidden - User lacks required permissions</response>
        /// <response code="404">Order not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [RequirePolicy("ORDER:VIEW")]
        [ProducesResponseType(typeof(ApiSuccessResult<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResult<OrderResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(long id)
        {
            var query = new GetOrderByIdQuery { Id = id };
            return await HandleGetByIdAsync<GetOrderByIdQuery, OrderResponseDto>(query, id, EntityName);
        }

        /// <summary>
        /// Create a new order - requires ORDER:CREATE policy
        /// </summary>
        /// <param name="dto">Order creation data</param>
        /// <returns>ID of the created order</returns>
        /// <response code="201">Order created successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="403">Forbidden - User lacks required permissions</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [RequirePolicy("ORDER:CREATE")]
        [ProducesResponseType(typeof(ApiSuccessResult<long>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] OrderCreateDto dto)
        {
            var command = new CreateOrderCommand
            {
                CustomerName = dto.CustomerName,
                OrderItems = dto.OrderItems
            };

            return await HandleCreateAsync(command, EntityName, dto.CustomerName);
        }

        /// <summary>
        /// Update an existing order - requires ORDER:UPDATE policy
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="dto">Order update data</param>
        /// <returns>Update result</returns>
        /// <response code="200">Order updated successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="403">Forbidden - User lacks required permissions</response>
        /// <response code="404">Order not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        [RequirePolicy("ORDER:UPDATE")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(long id, [FromBody] OrderUpdateDto dto)
        {
            var command = new UpdateOrderCommand
            {
                Id = dto.Id,
                CustomerName = dto.CustomerName,
                OrderItems = dto.OrderItems
            };

            return await HandleUpdateAsync(command, id, dto.Id, EntityName);
        }

        /// <summary>
        /// Delete an order - requires ORDER:DELETE policy
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <returns>Delete result</returns>
        /// <response code="200">Order deleted successfully</response>
        /// <response code="403">Forbidden - User lacks required permissions</response>
        /// <response code="404">Order not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [RequirePolicy("ORDER:DELETE")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id)
        {
            var command = new DeleteOrderCommand { Id = id };
            return await HandleDeleteAsync(command, id, EntityName);
        }
    }
}

