using ATMChallenge.Domain.Entities;

namespace ATMChallenge.Application.Interfaces
{
    public interface ICardRepository
    {
        Task<Card?> GetByCardNumberAsync(string cardNumber);
        Task UpdateAsync(Card card);
    }
}
