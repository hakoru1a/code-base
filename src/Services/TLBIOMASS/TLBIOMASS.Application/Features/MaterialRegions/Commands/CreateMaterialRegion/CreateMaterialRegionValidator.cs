using FluentValidation;

namespace TLBIOMASS.Application.Features.MaterialRegions.Commands.CreateMaterialRegion
{
    public class CreateMaterialRegionValidator : AbstractValidator<CreateMaterialRegionCommand>
    {
        public CreateMaterialRegionValidator()
        {
            RuleFor(x => x.RegionName)
                .NotEmpty().WithMessage("Region name is required")
                .MaximumLength(150).WithMessage("Region name must not exceed 150 characters");

            RuleFor(x => x.AreaHa)
                .GreaterThan(0).WithMessage("Area must be greater than 0");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");

            RuleFor(x => x.OwnerId)
                .NotEmpty().WithMessage("Owner ID is required");
                
            RuleForEach(x => x.RegionMaterials).ChildRules(material => {
                material.RuleFor(m => m.MaterialId).NotEmpty().WithMessage("Material ID is required");
            });
        }
    }
}
