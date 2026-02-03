using ATMChallenge.Application.DTOs.Responses;
using ATMChallenge.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ATMChallenge.Application.Features.Account
{
    public class GetOperationsHandler : IRequestHandler<GetOperationsQuery, PagedOperationsResponse>
    {
        private readonly IAccountRepository _accountRepository;
        private const int PageSize = 10;

        public GetOperationsHandler(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<PagedOperationsResponse> Handle(GetOperationsQuery request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByCardNumberAsync(request.CardNumber);
            if (account == null) throw new KeyNotFoundException("Card not found");

            var queryable = account.Operations!.AsQueryable().OrderByDescending(o => o.Timestamp);
            var total = queryable.Count();
            var items = queryable.Skip((request.Page - 1) * PageSize).Take(PageSize)
                .Select(o => new OperationDto(o.Type.ToString(), o.Amount, o.Timestamp)).ToList();

            return new PagedOperationsResponse(request.Page, PageSize, total, items);
        }
    }
}
