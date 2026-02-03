using System.Threading.Tasks;
using ATMChallenge.Application.Features.Account;
using ATMChallenge.Application.DTOs.Responses;
using ATMChallenge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ATMChallenge.Application.Interfaces;
using ATMChallenge.Infrastructure.Repositories;

namespace ATMChallenge.Tests
{
    public class GetBalanceHandlerTests
    {
        [Fact]
        public async Task GetBalance_ReturnsCorrectInfo()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            using var db = new AppDbContext(options);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var account = new ATMChallenge.Domain.Entities.Account { AccountNumber = "AR100", Balance = 750.50m, LastWithdrawal = System.DateTime.UtcNow.AddDays(-1), CreatedAt = System.DateTime.UtcNow };
            db.Accounts.Add(account);
            db.Users.Add(new ATMChallenge.Domain.Entities.User { FullName = "Balance User", AccountId = account.Id });
            db.Cards.Add(new ATMChallenge.Domain.Entities.Card { CardNumber = "4111111111111111", PinHash = BCrypt.Net.BCrypt.HashPassword("1111"), AccountId = account.Id, CreatedAt = System.DateTime.UtcNow });
            await db.SaveChangesAsync();

            IAccountRepository repo = new AccountRepository(db);
            var handler = new GetBalanceHandler(repo);
            var result = await handler.Handle(new GetBalanceQuery("4111111111111111"), System.Threading.CancellationToken.None);

            Assert.IsType<BalanceResponse>(result);
            Assert.Equal("Balance User", result.UserFullName);
            Assert.Equal("AR100", result.AccountNumber);
            Assert.Equal(750.50m, result.Balance);
            Assert.NotNull(result.LastWithdrawal);
        }
    }
}
