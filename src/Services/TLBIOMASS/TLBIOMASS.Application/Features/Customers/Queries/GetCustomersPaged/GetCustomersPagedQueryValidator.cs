using FluentValidation;
using Shared.DTOs.Customer;
using Shared.Validators;

namespace TLBIOMASS.Application.Features.Customers.Queries.GetCustomersPaged;

/// <summary>
/// Validator cho CustomerPagedFilterDto
/// </summary>
public class CustomerPagedFilterDtoValidator : BasePagedFilterValidator<CustomerPagedFilterDto>
{
    public CustomerPagedFilterDtoValidator()
    {
        // Base validator đã handle: PageNumber, PageSize, OrderBy, OrderByDirection
        
        // Custom validation cho Search field
        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search))
            .WithMessage("Search term không được vượt quá 200 ký tự");
    }

    /// <summary>
    /// Customer entity không có navigation properties nên không cần blacklist
    /// </summary>
    protected override string[] GetFieldsCanNotSort()
    {
        return Array.Empty<string>();
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
