using FluentValidation;

namespace Generate.Application.Features.Orders.Commands.UpdateOrder
{
    public class UpdateOrderValidator : AbstractValidator<UpdateOrderCommand>
    {
        public UpdateOrderValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required")
                .GreaterThan(0).WithMessage("Id must be greater than 0");

            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("CustomerName is required")
                .NotNull().WithMessage("CustomerName cannot be null")
                .MaximumLength(200).WithMessage("CustomerName must not exceed 200 characters");

            RuleFor(x => x.OrderItems)
                .NotEmpty().WithMessage("OrderItems is required")
                .NotNull().WithMessage("OrderItems cannot be null")
                .Must(items => items.Count > 0).WithMessage("Order must have at least one item");

            RuleForEach(x => x.OrderItems).ChildRules(item =>
            {
                item.RuleFor(x => x.ProductId)
                    .NotEmpty().WithMessage("ProductId is required")
                    .GreaterThan(0).WithMessage("ProductId must be greater than 0");

                item.RuleFor(x => x.Quantity)
                    .NotEmpty().WithMessage("Quantity is required")
                    .GreaterThan(0).WithMessage("Quantity must be greater than 0");
            });
        }
    }
}

