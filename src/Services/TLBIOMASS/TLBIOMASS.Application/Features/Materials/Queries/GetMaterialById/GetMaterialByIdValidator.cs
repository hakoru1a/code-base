using FluentValidation;

namespace TLBIOMASS.Application.Features.Materials.Queries.GetMaterialById
{
    public class GetMaterialByIdValidator : AbstractValidator<GetMaterialByIdQuery>
    {
        public GetMaterialByIdValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required")
                .GreaterThan(0).WithMessage("Id must be greater than 0");
        }
    }
}
