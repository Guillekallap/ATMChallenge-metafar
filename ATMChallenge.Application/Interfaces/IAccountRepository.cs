using ATMChallenge.Domain.Entities;

namespace ATMChallenge.Application.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(int id);
        Task<Account?> GetByCardNumberAsync(string cardNumber);
        Task AddOperationAsync(Operation operation);
        Task<User?> GetUserByAccountIdAsync(int accountId);
        // Intenta realizar el retiro de forma transaccional/optimista.
        // Retorna true si el retiro se realizó correctamente; false si fondos insuficientes o conflicto.
        Task<bool> TryWithdrawAsync(int accountId, decimal amount, CancellationToken cancellationToken);
    }
}
