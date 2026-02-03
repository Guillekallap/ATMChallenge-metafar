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
using System;

namespace ATMChallenge.Tests
{
    public class AuthHandlerTests
    {
        [Fact]
        public async Task Login_WithValidPin_ReturnsToken()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var db = new AppDbContext(options);

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // Seed similar to production seed
            var account = new ATMChallenge.Domain.Entities.Account { AccountNumber = "AR123", Balance = 1000m, CreatedAt = DateTime.UtcNow };
            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            db.Users.Add(new ATMChallenge.Domain.Entities.User { FullName = "Test User", AccountId = account.Id });
            var hashedPin = BCrypt.Net.BCrypt.HashPassword("1234");
            db.Cards.Add(new ATMChallenge.Domain.Entities.Card { CardNumber = "4000000000000001", PinHash = hashedPin, AccountId = account.Id, IsBlocked = false, FailedPinAttempts = 0, CreatedAt = DateTime.UtcNow });
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
            var result = await handler.Handle(new LoginCommand("4000000000000001", "1234"), CancellationToken.None);

            Assert.True(result.Success);
            Assert.False(string.IsNullOrEmpty(result.Token));
        }
    }
}
