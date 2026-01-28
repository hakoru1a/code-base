using FluentValidation;

namespace TLBIOMASS.Application.Features.MaterialRegions.Queries.GetMaterialRegionById
{
    public class GetMaterialRegionByIdValidator : AbstractValidator<GetMaterialRegionByIdQuery>
    {
        public GetMaterialRegionByIdValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required")
                .GreaterThan(0).WithMessage("Id must be greater than 0");
        }
    }
}
