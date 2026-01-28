using FluentValidation;

namespace TLBIOMASS.Application.Features.Receivers.Queries.GetReceiverById
{
    public class GetReceiverByIdValidator : AbstractValidator<GetReceiverByIdQuery>
    {
        public GetReceiverByIdValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required")
                .GreaterThan(0).WithMessage("Id must be greater than 0");
        }
    }
}
