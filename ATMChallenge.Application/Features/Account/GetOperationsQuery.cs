using MediatR;
using ATMChallenge.Application.DTOs.Responses;

namespace ATMChallenge.Application.Features.Account
{
    public record GetOperationsQuery(string CardNumber, int Page = 1) : IRequest<PagedOperationsResponse>;
}
