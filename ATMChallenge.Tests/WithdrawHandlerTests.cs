using System.Threading.Tasks;
using ATMChallenge.Application.Features.Account;
using ATMChallenge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ATMChallenge.Application.Interfaces;
using ATMChallenge.Infrastructure.Repositories;

namespace ATMChallenge.Tests
{
    public class WithdrawHandlerTests
    {
        [Fact]
        public async Task Withdraw_WithSufficientFunds_Succeeds()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            using var db = new AppDbContext(options);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var account = new ATMChallenge.Domain.Entities.Account { AccountNumber = "AR1", Balance = 500m };
            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            db.Users.Add(new ATMChallenge.Domain.Entities.User { FullName = "User", AccountId = account.Id });
            db.Cards.Add(new ATMChallenge.Domain.Entities.Card { CardNumber = "4000", PinHash = "x", AccountId = account.Id });
            await db.SaveChangesAsync();

            IAccountRepository repo = new AccountRepository(db);
            var handler = new WithdrawHandler(repo);
            var result = await handler.Handle(new WithdrawCommand("4000", 200m), System.Threading.CancellationToken.None);

            Assert.True(result.Success);
            Assert.Equal(300m, result.NewBalance);
        }

        [Fact]
        public async Task Withdraw_WithInsufficientFunds_Fails()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            using var db = new AppDbContext(options);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var account = new ATMChallenge.Domain.Entities.Account { AccountNumber = "AR1", Balance = 100m };
            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            db.Users.Add(new ATMChallenge.Domain.Entities.User { FullName = "User", AccountId = account.Id });
            db.Cards.Add(new ATMChallenge.Domain.Entities.Card { CardNumber = "4000", PinHash = "x", AccountId = account.Id });
            await db.SaveChangesAsync();

            IAccountRepository repo = new AccountRepository(db);
            var handler = new WithdrawHandler(repo);
            var result = await handler.Handle(new WithdrawCommand("4000", 200m), System.Threading.CancellationToken.None);

            Assert.False(result.Success);
            Assert.Equal(100m, result.NewBalance);
        }
    }
}
