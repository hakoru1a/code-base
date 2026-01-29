using FluentValidation;

namespace TLBIOMASS.Application.Features.Landowners.Commands.UpdateLandowner
{
    public class UpdateLandownerValidator : AbstractValidator<UpdateLandownerCommand>
    {
        public UpdateLandownerValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(150).WithMessage("Name must not exceed 150 characters");

            RuleFor(x => x.Phone)
                .MaximumLength(50).WithMessage("Phone must not exceed 50 characters");

            RuleFor(x => x.Email)
                .MaximumLength(100).WithMessage("Email must not exceed 100 characters")
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Invalid email format");

            RuleFor(x => x.Address)
                .MaximumLength(255).WithMessage("Address must not exceed 255 characters");


            RuleFor(x => x.IdentityCardNo)
                .MaximumLength(20).WithMessage("Identity card must not exceed 20 characters");

            RuleFor(x => x.IssuePlace)
                .MaximumLength(255).WithMessage("Issue place must not exceed 255 characters");
        }
    }
}
