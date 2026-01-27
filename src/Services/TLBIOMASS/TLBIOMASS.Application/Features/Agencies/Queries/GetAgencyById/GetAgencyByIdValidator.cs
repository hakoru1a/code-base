using FluentValidation;

namespace TLBIOMASS.Application.Features.Agencies.Queries.GetAgencyById
{
    public class GetAgencyByIdValidator : AbstractValidator<GetAgencyByIdQuery>
    {
        public GetAgencyByIdValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required")
                .GreaterThan(0).WithMessage("Id must be greater than 0");
        }
    }
}
