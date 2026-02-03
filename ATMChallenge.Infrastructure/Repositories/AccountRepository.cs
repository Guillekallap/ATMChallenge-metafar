using ATMChallenge.Application.Interfaces;
using ATMChallenge.Domain.Entities;
using ATMChallenge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ATMChallenge.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _db;

        public AccountRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Account?> GetByIdAsync(int id)
        {
            return await _db.Accounts.Include(a => a.Cards).Include(a => a.Operations).FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Account?> GetByCardNumberAsync(string cardNumber)
        {
            var card = await _db.Cards.Include(c => c.Account).Include(c => c.Account!.Operations).FirstOrDefaultAsync(c => c.CardNumber == cardNumber);
            return card?.Account;
        }

        public async Task AddOperationAsync(Operation operation)
        {
            _db.Operations.Add(operation);
            await _db.SaveChangesAsync();
        }

        public async Task<User?> GetUserByAccountIdAsync(int accountId)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.AccountId == accountId);
        }

        public async Task<bool> TryWithdrawAsync(int accountId, decimal amount, CancellationToken cancellationToken)
        {
            // Intentar en bucle por si hay conflictos de concurrencia
            for (int attempt = 0; attempt < 3; attempt++)
            {
                var account = await _db.Accounts.Include(a => a.Operations).FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
                if (account == null) return false;

                if (account.Balance < amount) return false;

                account.Balance -= amount;
                account.LastWithdrawal = DateTime.UtcNow;

                var operation = new Operation
                {
                    AccountId = accountId,
                    Amount = amount,
                    Timestamp = DateTime.UtcNow,
                    Type = OperationType.Withdrawal,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Operations.Add(operation);

                try
                {
                    await _db.SaveChangesAsync(cancellationToken);
                    return true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Concurrency conflict, reintentar
                    continue;
                }
            }

            return false;
        }
    }
}
