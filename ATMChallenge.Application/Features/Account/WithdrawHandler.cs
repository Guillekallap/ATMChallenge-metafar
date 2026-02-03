using ATMChallenge.Application.Interfaces;
using MediatR;

namespace ATMChallenge.Application.Features.Account
{
    public class WithdrawHandler : IRequestHandler<WithdrawCommand, WithdrawResult>
    {
        private readonly IAccountRepository _accountRepository;

        public WithdrawHandler(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<WithdrawResult> Handle(WithdrawCommand request, CancellationToken cancellationToken)
        {
            // Validaciones básicas
            if (request.Amount <= 0) return new WithdrawResult(false, 0m, DateTime.UtcNow, "Amount must be greater than zero");

            // Obtener cuenta y aplicar retiro en transacción para evitar condiciones de carrera
            var account = await _accountRepository.GetByCardNumberAsync(request.CardNumber);
            if (account == null) return new WithdrawResult(false, 0m, DateTime.UtcNow, "Card not found");

            if (account.Cards == null) return new WithdrawResult(false, 0m, DateTime.UtcNow, "Card not found");

            var card = account.Cards.FirstOrDefault(c => c.CardNumber == request.CardNumber);
            if (card == null) return new WithdrawResult(false, 0m, DateTime.UtcNow, "Card not found");
            if (card.IsBlocked) return new WithdrawResult(false, 0m, DateTime.UtcNow, "Card is blocked");

            // Ejecutar retiro dentro de transacción optimista usando RowVersion
            try
            {
                var success = await _accountRepository.TryWithdrawAsync(account.Id, request.Amount, cancellationToken);
                if (!success) return new WithdrawResult(false, account.Balance, DateTime.UtcNow, "Insufficient funds or concurrency conflict");

                var updatedAccount = await _accountRepository.GetByIdAsync(account.Id);
                return new WithdrawResult(true, updatedAccount!.Balance, DateTime.UtcNow, null);
            }
            catch (Exception ex)
            {
                return new WithdrawResult(false, account.Balance, DateTime.UtcNow, ex.Message);
            }
        }
    }
}
