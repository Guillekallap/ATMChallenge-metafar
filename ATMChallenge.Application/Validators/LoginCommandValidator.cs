using ATMChallenge.Application.Features.Auth;
using FluentValidation;

namespace ATMChallenge.Application.Validators
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.CardNumber).NotEmpty().Length(13, 19).WithMessage("El número de tarjeta es requerido y debe tener entre 13 y 19 dígitos.");
            RuleFor(x => x.Pin).NotEmpty().Length(4, 6).WithMessage("El PIN es requerido y debe tener entre 4 y 6 dígitos.");
        }
    }
}
