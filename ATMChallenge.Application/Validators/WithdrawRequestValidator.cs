using ATMChallenge.Application.DTOs.Requests;
using FluentValidation;

namespace ATMChallenge.Application.Validators
{
    public class WithdrawRequestValidator : AbstractValidator<WithdrawRequest>
    {
        public WithdrawRequestValidator()
        {
            RuleFor(x => x.CardNumber).NotEmpty().Length(13, 19).WithMessage("El número de tarjeta debe tener entre 13 y 19 dígitos.");
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("El monto debe ser mayor a cero.");
            RuleFor(x => x.Amount).LessThanOrEqualTo(1000000m).WithMessage("El monto excede el límite permitido.");
        }
    }
}
