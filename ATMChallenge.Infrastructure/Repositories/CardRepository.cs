using ATMChallenge.Application.Interfaces;
using ATMChallenge.Domain.Entities;
using ATMChallenge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ATMChallenge.Infrastructure.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly AppDbContext _db;

        public CardRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Card?> GetByCardNumberAsync(string cardNumber)
        {
            return await _db.Cards.Include(c => c.Account).FirstOrDefaultAsync(c => c.CardNumber == cardNumber);
        }

        public async Task UpdateAsync(Card card)
        {
            _db.Cards.Update(card);
            await _db.SaveChangesAsync();
        }
    }
}
