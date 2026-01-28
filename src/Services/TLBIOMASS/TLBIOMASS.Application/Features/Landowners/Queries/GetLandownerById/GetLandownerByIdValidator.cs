using FluentValidation;

namespace TLBIOMASS.Application.Features.Landowners.Queries.GetLandownerById
{
    public class GetLandownerByIdValidator : AbstractValidator<GetLandownerByIdQuery>
    {
        public GetLandownerByIdValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required")
                .GreaterThan(0).WithMessage("Id must be greater than 0");
        }
    }
}
