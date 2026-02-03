using MediatR;
using ATMChallenge.Application.DTOs.Responses;

namespace ATMChallenge.Application.Features.Account
{
    public record GetBalanceQuery(string CardNumber) : IRequest<BalanceResponse>;
}
