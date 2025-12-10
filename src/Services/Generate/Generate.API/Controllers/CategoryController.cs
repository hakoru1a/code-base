using Asp.Versioning;
using Shared.DTOs.Category;
using Generate.Application.Features.Categories.Commands.CreateCategory;
using Generate.Application.Features.Categories.Commands.DeleteCategory;
using Generate.Application.Features.Categories.Commands.UpdateCategory;
using Generate.Application.Features.Categories.Queries.GetCategories;
using Generate.Application.Features.Categories.Queries.GetCategoriesPaged;
using Generate.Application.Features.Categories.Queries.GetCategoryById;
using Infrastructure.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Identity;
using Shared.SeedWork;

namespace Generate.API.Controllers
{
    /// <summary>
    /// Controller for managing categories
    /// </summary>
    [ApiVersion("2.0")]
    public class CategoryController : ApiControllerBase<CategoryController>
    {
        private const string EntityName = "Category";

        public CategoryController(IMediator mediator, ILogger<CategoryController> logger)
            : base(mediator, logger)
        {
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
        // [Authorize(Policy = PolicyNames.Hybrid.Category.CanView)]
        public async Task<IActionResult> GetList()
        {
            var query = new GetCategoriesQuery();
            return await HandleGetAllAsync<GetCategoriesQuery, CategoryResponseDto>(query, EntityName);
        }

        /// <summary>
        /// Get paginated list of categories with filtering and sorting
        /// </summary>
        /// <param name="filter">Filter parameters including pagination, search, and ordering</param>
        /// <returns>Paginated list of categories</returns>
        /// <response code="200">Returns the paginated list of categories</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(ApiSuccessResult<List<CategoryResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPagedList([FromQuery] CategoryFilterDto filter)
        {
            var query = new GetCategoriesPagedQuery { Filter = filter };
            return await HandleGetPagedAsync<GetCategoriesPagedQuery, CategoryResponseDto>(
                query, EntityName, filter.PageNumber, filter.PageSize);
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
            var query = new GetCategoryByIdQuery { Id = id };
            return await HandleGetByIdAsync<GetCategoryByIdQuery, CategoryResponseDto>(query, id, EntityName);
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
            var command = new CreateCategoryCommand
            {
                Name = dto.Name
            };

            return await HandleCreateAsync(command, EntityName, dto.Name);
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
            var command = new UpdateCategoryCommand
            {
                Id = dto.Id,
                Name = dto.Name
            };

            return await HandleUpdateAsync(command, id, dto.Id, EntityName);
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
            var command = new DeleteCategoryCommand { Id = id };
            return await HandleDeleteAsync(command, id, EntityName);
        }
    }
}

