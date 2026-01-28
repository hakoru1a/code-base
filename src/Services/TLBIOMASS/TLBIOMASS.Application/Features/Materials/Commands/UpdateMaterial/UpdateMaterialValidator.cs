using FluentValidation;

namespace TLBIOMASS.Application.Features.Materials.Commands.UpdateMaterial
{
    public class UpdateMaterialValidator : AbstractValidator<UpdateMaterialCommand>
    {
        public UpdateMaterialValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Material name is required")
                .MaximumLength(200).WithMessage("Material name must not exceed 200 characters");

            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage("Unit is required")
                .MaximumLength(50).WithMessage("Unit must not exceed 50 characters");

            RuleFor(x => x.ProposedImpurityDeduction)
                .GreaterThanOrEqualTo(0).WithMessage("Proposed impurity deduction must be greater than or equal to 0")
                .LessThanOrEqualTo(100).WithMessage("Proposed impurity deduction must be less than or equal to 100");
        }
    }
}
