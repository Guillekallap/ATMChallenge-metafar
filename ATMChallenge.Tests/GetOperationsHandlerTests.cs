using System.Linq;
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
    public class GetOperationsHandlerTests
    {
        [Fact]
        public async Task GetOperations_ReturnsPaginatedItems()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            using var db = new AppDbContext(options);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var account = new ATMChallenge.Domain.Entities.Account { AccountNumber = "AR200", Balance = 1000m, CreatedAt = System.DateTime.UtcNow };
            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            db.Users.Add(new ATMChallenge.Domain.Entities.User { FullName = "Ops User", AccountId = account.Id });
            db.Cards.Add(new ATMChallenge.Domain.Entities.Card { CardNumber = "4222222222222", PinHash = "x", AccountId = account.Id, CreatedAt = System.DateTime.UtcNow });

            for (int i = 1; i <= 25; i++)
            {
                db.Operations.Add(new ATMChallenge.Domain.Entities.Operation { AccountId = account.Id, Amount = i * 10, Timestamp = System.DateTime.UtcNow.AddDays(-i), Type = ATMChallenge.Domain.Entities.OperationType.Withdrawal, CreatedAt = System.DateTime.UtcNow });
            }

            await db.SaveChangesAsync();

            IAccountRepository repo = new AccountRepository(db);
            var handler = new GetOperationsHandler(repo);
            var page1 = await handler.Handle(new GetOperationsQuery("4222222222222", 1), System.Threading.CancellationToken.None);
            var page3 = await handler.Handle(new GetOperationsQuery("4222222222222", 3), System.Threading.CancellationToken.None);

            Assert.IsType<PagedOperationsResponse>(page1);
            Assert.Equal(10, page1.Items.Count);
            Assert.Equal(25, page1.TotalItems);

            Assert.Equal(5, page3.Items.Count);
        }
    }
}
