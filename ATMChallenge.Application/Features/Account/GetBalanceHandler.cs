using ATMChallenge.Application.DTOs.Responses;
using ATMChallenge.Application.Interfaces;
using MediatR;

namespace ATMChallenge.Application.Features.Account
{
    public class GetBalanceHandler : IRequestHandler<GetBalanceQuery, BalanceResponse>
    {
        private readonly IAccountRepository _accountRepository;

        public GetBalanceHandler(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<BalanceResponse> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByCardNumberAsync(request.CardNumber);

            if (account == null) throw new KeyNotFoundException("Card not found");

            var user = await _accountRepository.GetUserByAccountIdAsync(account.Id);
            if (user == null) throw new KeyNotFoundException("User not found");

            return new BalanceResponse(user.FullName, account.AccountNumber, account.Balance, account.LastWithdrawal);
        }
    }
}
