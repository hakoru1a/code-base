using FluentValidation;
using Shared.DTOs.Category;

namespace Generate.Application.Features.Categories.Queries.GetCategoriesPaged;

/// <summary>
/// Validator cho CategoryFilterDto
/// </summary>
public class CategoryFilterDtoValidator : AbstractValidator<CategoryFilterDto>
{
    private static readonly string[] FieldsCanNotSort = { "Products" };

    public CategoryFilterDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber phải lớn hơn 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize phải lớn hơn 0")
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize không được vượt quá 100");

        RuleFor(x => x.OrderBy)
            .Must(BeValidSortField)
            .When(x => !string.IsNullOrWhiteSpace(x.OrderBy))
            .WithMessage(x => GetInvalidSortFieldMessage(x.OrderBy));

        RuleFor(x => x.OrderByDirection)
            .Must(direction => string.IsNullOrWhiteSpace(direction) ||
                              direction.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
                              direction.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("OrderByDirection chỉ được là 'asc' hoặc 'desc'");

        RuleFor(x => x.Name)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Name))
            .WithMessage("Tên category không được vượt quá 200 ký tự");

        RuleFor(x => x)
            .Must(x => !x.CreatedFrom.HasValue || !x.CreatedTo.HasValue || x.CreatedFrom!.Value <= x.CreatedTo!.Value)
            .WithMessage("CreatedFrom phải nhỏ hơn hoặc bằng CreatedTo");
    }

    private static bool BeValidSortField(string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy)) return true;
        return !FieldsCanNotSort.Any(f => f.Equals(orderBy, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetInvalidSortFieldMessage(string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy)) return string.Empty;
        return $"'{orderBy}' không được phép sort. Các fields không được sort: {string.Join(", ", FieldsCanNotSort)}";
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
