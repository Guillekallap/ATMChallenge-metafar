using MediatR;

namespace ATMChallenge.Application.Features.Auth
{
    public record LoginCommand(string CardNumber, string Pin) : IRequest<LoginResult>;

    public record LoginResult(bool Success, string? Token, string? Message);
}
