using FluentValidation;
using TableFlow.Web.Models;

namespace TableFlow.Web.Validators
{
    public class UserFormValidator : AbstractValidator<UserFormModel>
    {
        public UserFormValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MinimumLength(2).WithMessage("Name must be at least 2 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Enter a valid email address.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Please select a role.")
                .Must(r => r == "Cashier" || r == "Kitchen")
                .WithMessage("Role must be Cashier or Kitchen.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
                .Matches("[A-Z]").WithMessage("Must contain at least one uppercase letter.")
                .Matches("[0-9]").WithMessage("Must contain at least one number.")
                .When(x => x.IsEditMode == false);

            RuleFor(x => x.NewPassword)
                .MinimumLength(6).WithMessage("New password must be at least 6 characters.")
                .Matches("[A-Z]").WithMessage("Must contain at least one uppercase letter.")
                .Matches("[0-9]").WithMessage("Must contain at least one number.")
                .When(x => x.IsEditMode && !string.IsNullOrEmpty(x.NewPassword));
        }
    }
}
