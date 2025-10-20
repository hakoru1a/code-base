using Generate.Application.Common.DTOs.Category;
using Generate.Application.Features.Category.Commands.CreateCategory;
using Generate.Application.Features.Category.Commands.DeleteCategory;
using Generate.Application.Features.Category.Commands.UpdateCategory;
using Generate.Application.Features.Category.Queries.GetCategories;
using Generate.Application.Features.Category.Queries.GetCategoryById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.SeedWork;

namespace Generate.API.Controllers
{
    /// <summary>
    /// Controller for managing categories
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(IMediator mediator, ILogger<CategoryController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns>List of categories</returns>
        /// <response code="200">Returns the list of categories</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiSuccessResult<List<CategoryResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetList()
        {
            _logger.LogInformation("Getting all categories");
            var query = new GetCategoriesQuery();
            var result = await _mediator.Send(query);
            return Ok(new ApiSuccessResult<List<CategoryResponseDto>>(result));
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Category details</returns>
        /// <response code="200">Returns the category</response>
        /// <response code="404">Category not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<CategoryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<CategoryResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(long id)
        {
            _logger.LogInformation("Getting category with ID: {CategoryId}", id);
            var query = new GetCategoryByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new ApiErrorResult<CategoryResponseDto>($"Category with ID {id} not found"));
            }

            return Ok(new ApiSuccessResult<CategoryResponseDto>(result));
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        /// <param name="dto">Category creation data</param>
        /// <returns>ID of the created category</returns>
        /// <response code="201">Category created successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiSuccessResult<long>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
        {
            _logger.LogInformation("Creating new category: {CategoryName}", dto.Name);

            var command = new CreateCategoryCommand
            {
                Name = dto.Name
            };

            var categoryId = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetById),
                new { id = categoryId },
                new ApiSuccessResult<long>(categoryId, "Category created successfully")
            );
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="dto">Category update data</param>
        /// <returns>Update result</returns>
        /// <response code="200">Category updated successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="404">Category not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(long id, [FromBody] CategoryUpdateDto dto)
        {
            _logger.LogInformation("Updating category with ID: {CategoryId}", id);

            if (id != dto.Id)
            {
                return BadRequest(new ApiErrorResult<bool>("ID in URL does not match ID in body"));
            }

            var command = new UpdateCategoryCommand
            {
                Id = dto.Id,
                Name = dto.Name
            };

            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound(new ApiErrorResult<bool>($"Category with ID {id} not found"));
            }

            return Ok(new ApiSuccessResult<bool>(result, "Category updated successfully"));
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Delete result</returns>
        /// <response code="200">Category deleted successfully</response>
        /// <response code="404">Category not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id)
        {
            _logger.LogInformation("Deleting category with ID: {CategoryId}", id);

            var command = new DeleteCategoryCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound(new ApiErrorResult<bool>($"Category with ID {id} not found"));
            }

            return Ok(new ApiSuccessResult<bool>(result, "Category deleted successfully"));
        }
    }
}

