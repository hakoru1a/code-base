using FluentValidation;
using Shared.DTOs.Category;
using Shared.Validators;

namespace Generate.Application.Features.Categories.Queries.GetCategoriesPaged;

/// <summary>
/// Validator cho CategoryFilterDto
/// </summary>
public class CategoryFilterDtoValidator : BasePagedFilterValidator<CategoryFilterDto>
{
    public CategoryFilterDtoValidator()
    {
        // Base validator đã handle: PageNumber, PageSize, OrderBy, OrderByDirection
        
        // Thêm validation cho các filter fields cụ thể của Category (nếu cần)
        RuleFor(x => x.Name)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Name))
            .WithMessage("Tên category không được vượt quá 200 ký tự");

        // Validate date range
        RuleFor(x => x)
            .Must(x => !x.CreatedFrom.HasValue || !x.CreatedTo.HasValue || x.CreatedFrom.Value <= x.CreatedTo.Value)
            .WithMessage("CreatedFrom phải nhỏ hơn hoặc bằng CreatedTo");
    }

    /// <summary>
    /// Định nghĩa các fields KHÔNG được sort
    /// Ví dụ: navigation properties như Products không nên sort
    /// </summary>
    protected override string[] GetFieldsCanNotSort()
    {
        return new[]
        {
            "Products" // Navigation property, không nên sort
        };
    }
}

/// <summary>
/// Validator cho GetCategoriesPagedQuery
/// </summary>
public class GetCategoriesPagedQueryValidator : AbstractValidator<GetCategoriesPagedQuery>
{
    public GetCategoriesPagedQueryValidator()
    {
        RuleFor(x => x.Filter)
            .NotNull()
            .WithMessage("Filter không được null")
            .SetValidator(new CategoryFilterDtoValidator());
    }
}
