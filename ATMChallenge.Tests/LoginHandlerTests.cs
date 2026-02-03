using System.Threading;
using System.Threading.Tasks;
using ATMChallenge.Application.Features.Auth;
using ATMChallenge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using ATMChallenge.Application.Interfaces;
using ATMChallenge.Infrastructure.Repositories;

namespace ATMChallenge.Tests
{
    public class LoginHandlerTests
    {
        [Fact]
        public async Task Login_WithInvalidPin_IncrementsFailedAttemptsAndBlocksAfterFour()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            using var db = new AppDbContext(options);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var account = new ATMChallenge.Domain.Entities.Account { AccountNumber = "AR1", Balance = 100m, CreatedAt = System.DateTime.UtcNow };
            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            db.Users.Add(new ATMChallenge.Domain.Entities.User { FullName = "User", AccountId = account.Id });
            var hashedPin = BCrypt.Net.BCrypt.HashPassword("1234");
            db.Cards.Add(new ATMChallenge.Domain.Entities.Card { CardNumber = "4000", PinHash = hashedPin, AccountId = account.Id, IsBlocked = false, FailedPinAttempts = 0, CreatedAt = System.DateTime.UtcNow });
            await db.SaveChangesAsync();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Key", new string('K', 64) },
                    { "Jwt:Issuer", "TestIssuer" },
                    { "Jwt:Audience", "TestAudience" },
                    { "Jwt:ExpireMinutes", "60" }
                }).Build();

            ICardRepository repo = new CardRepository(db);
            var handler = new LoginHandler(repo, configuration);

            // Try 3 invalid attempts
            for (int i = 0; i < 3; i++)
            {
                var res = await handler.Handle(new LoginCommand("4000", "0000"), CancellationToken.None);
                Assert.False(res.Success);
                Assert.Equal("Invalid PIN", res.Message);
            }

            // Check failed attempts is 3
            var card = await db.Cards.FirstOrDefaultAsync(c => c.CardNumber == "4000");
            Assert.NotNull(card);
            Assert.Equal(3, card.FailedPinAttempts);
            Assert.False(card.IsBlocked);

            // Fourth invalid attempt should block the card
            var res4 = await handler.Handle(new LoginCommand("4000", "0000"), CancellationToken.None);
            Assert.False(res4.Success);
            Assert.Equal("Invalid PIN", res4.Message);

            card = await db.Cards.FirstOrDefaultAsync(c => c.CardNumber == "4000");
            Assert.NotNull(card);
            Assert.True(card.IsBlocked);

            // Further attempts should return blocked message
            var resAfter = await handler.Handle(new LoginCommand("4000", "1234"), CancellationToken.None);
            Assert.False(resAfter.Success);
            Assert.Equal("Card is blocked", resAfter.Message);
        }

        [Fact]
        public async Task Login_WithValidPin_ResetsFailedAttemptsAndReturnsToken()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            using var db = new AppDbContext(options);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var account = new ATMChallenge.Domain.Entities.Account { AccountNumber = "AR1", Balance = 100m, CreatedAt = System.DateTime.UtcNow };
            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            db.Users.Add(new ATMChallenge.Domain.Entities.User { FullName = "User", AccountId = account.Id });
            var hashedPin = BCrypt.Net.BCrypt.HashPassword("1234");
            db.Cards.Add(new ATMChallenge.Domain.Entities.Card { CardNumber = "4000", PinHash = hashedPin, AccountId = account.Id, IsBlocked = false, FailedPinAttempts = 2, CreatedAt = System.DateTime.UtcNow });
            await db.SaveChangesAsync();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Key", new string('K', 64) },
                    { "Jwt:Issuer", "TestIssuer" },
                    { "Jwt:Audience", "TestAudience" },
                    { "Jwt:ExpireMinutes", "60" }
                }).Build();

            ICardRepository repo = new CardRepository(db);
            var handler = new LoginHandler(repo, configuration);

            var res = await handler.Handle(new LoginCommand("4000", "1234"), CancellationToken.None);
            Assert.True(res.Success);
            Assert.False(string.IsNullOrEmpty(res.Token));

            var card = await db.Cards.FirstOrDefaultAsync(c => c.CardNumber == "4000");
            Assert.NotNull(card);
            Assert.Equal(0, card.FailedPinAttempts);
            Assert.False(card.IsBlocked);
        }
    }
}
