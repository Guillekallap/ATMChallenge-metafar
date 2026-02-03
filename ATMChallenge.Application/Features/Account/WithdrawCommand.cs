using MediatR;

namespace ATMChallenge.Application.Features.Account
{
    public record WithdrawCommand(string CardNumber, decimal Amount) : IRequest<WithdrawResult>;

    public record WithdrawResult(bool Success, decimal NewBalance, DateTime Timestamp, string? Message);
}
