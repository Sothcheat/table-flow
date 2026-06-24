using FluentValidation;
using TableFlow.Web.Models;

namespace TableFlow.Web.Validators;

public class LoginValidator : AbstractValidator<LoginModel>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
