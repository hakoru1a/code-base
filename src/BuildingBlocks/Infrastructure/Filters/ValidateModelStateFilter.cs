using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.SeedWork;

namespace Infrastructure.Filters
{
    /// <summary>
    /// Filter to validate model state and return ApiErrorResult for validation errors
    /// </summary>
    public class ValidateModelStateFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e =>
                        string.IsNullOrEmpty(e.ErrorMessage)
                            ? e.Exception?.Message ?? "Validation error"
                            : e.ErrorMessage))
                    .ToList();

                var result = new ApiErrorResult<object>(
                    ResponseMessages.ValidationFailed,
                    errors
                );

                context.Result = new BadRequestObjectResult(result);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No action needed after execution
        }
    }
}

