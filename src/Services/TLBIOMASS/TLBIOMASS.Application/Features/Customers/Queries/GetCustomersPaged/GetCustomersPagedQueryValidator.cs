using FluentValidation;
using Shared.DTOs.Customer;

namespace TLBIOMASS.Application.Features.Customers.Queries.GetCustomersPaged;

/// <summary>
/// Validator cho CustomerPagedFilterDto
/// </summary>
public class CustomerPagedFilterDtoValidator : AbstractValidator<CustomerPagedFilterDto>
{
    private static readonly string[] FieldsCanNotSort = Array.Empty<string>();

    public CustomerPagedFilterDtoValidator()
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

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search))
            .WithMessage("Search term không được vượt quá 200 ký tự");
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
/// Validator cho GetCustomersPagedQuery
/// </summary>
public class GetCustomersPagedQueryValidator : AbstractValidator<GetCustomersPagedQuery>
{
    public GetCustomersPagedQueryValidator()
    {
        RuleFor(x => x.Filter)
            .NotNull()
            .WithMessage("Filter không được null")
            .SetValidator(new CustomerPagedFilterDtoValidator());
    }
}
