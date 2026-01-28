using FluentValidation;
using Shared.DTOs;

namespace Shared.Validators;

/// <summary>
/// Base validator cho tất cả các paged query filters
/// </summary>
/// <typeparam name="TFilter">Filter DTO type</typeparam>
public abstract class BasePagedFilterValidator<TFilter> : AbstractValidator<TFilter>
    where TFilter : BaseFilterDto
{
    protected BasePagedFilterValidator()
    {
        // Validate pagination
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber phải lớn hơn 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize phải lớn hơn 0")
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize không được vượt quá 100");

        // Validate OrderBy field
        RuleFor(x => x.OrderBy)
            .Must(BeValidSortField)
            .When(x => !string.IsNullOrWhiteSpace(x.OrderBy))
            .WithMessage(x => GetInvalidSortFieldMessage(x.OrderBy));

        // Validate OrderByDirection
        RuleFor(x => x.OrderByDirection)
            .Must(direction => string.IsNullOrWhiteSpace(direction) || 
                              direction.Equals("asc", StringComparison.OrdinalIgnoreCase) || 
                              direction.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("OrderByDirection chỉ được là 'asc' hoặc 'desc'");
    }

    /// <summary>
    /// Override method này để định nghĩa các fields KHÔNG được sort (blacklist)
    /// Mặc định: tất cả fields đều được sort
    /// </summary>
    /// <returns>Array các field names KHÔNG được sort (ví dụ: navigation properties, collections)</returns>
    protected virtual string[] GetFieldsCanNotSort()
    {
        return Array.Empty<string>();
    }

    /// <summary>
    /// Kiểm tra xem field có hợp lệ để sort không
    /// </summary>
    private bool BeValidSortField(string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return true; // Cho phép null/empty
        }

        var fieldsCanNotSort = GetFieldsCanNotSort();
        // Nếu field KHÔNG nằm trong blacklist thì hợp lệ
        return !fieldsCanNotSort.Any(f => f.Equals(orderBy, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Lấy error message cho invalid sort field
    /// </summary>
    private string GetInvalidSortFieldMessage(string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return string.Empty;
        }

        var fieldsCanNotSort = GetFieldsCanNotSort();
        return $"'{orderBy}' không được phép sort. Các fields không được sort: {string.Join(", ", fieldsCanNotSort)}";
    }
}

