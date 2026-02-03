using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ATMChallenge.Application.DTOs.Requests;
using ATMChallenge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace ATMChallenge.Tests.Integration
{
    // Use Program (parcial pública) como entrypoint para WebApplicationFactory
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private const string DbName = "ATMChallenge_TestDb";

        private readonly WebApplicationFactory<Program> _factory;

        public ApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string?>("Jwt:Key", new string('K', 64)),
                        new KeyValuePair<string, string?>("Jwt:Issuer", "TestIssuer"),
                        new KeyValuePair<string, string?>("Jwt:Audience", "TestAudience"),
                        new KeyValuePair<string, string?>("Jwt:ExpireMinutes", "60")
                    });
                });

                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(DbName));

                    // Reconfigurar autenticación JWT para que use la misma key/issuer/audience que el LoginHandler genera
                    services.PostConfigureAll<JwtBearerOptions>(o =>
                    {
                        o.TokenValidationParameters.ValidIssuer = "TestIssuer";
                        o.TokenValidationParameters.ValidAudience = "TestAudience";
                        o.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(new string('K', 64)));
                    });
                });
            });
        }

        private record TokenResponse(string Token);

        private async Task SeedAsync(string cardNumber, string pin, decimal balance = 500m)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var account = new ATMChallenge.Domain.Entities.Account
            {
                AccountNumber = "AR100",
                Balance = balance,
                CreatedAt = DateTime.UtcNow
            };

            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            db.Users.Add(new ATMChallenge.Domain.Entities.User { FullName = "Integration User", AccountId = account.Id });
            db.Cards.Add(new ATMChallenge.Domain.Entities.Card
            {
                CardNumber = cardNumber,
                PinHash = BCrypt.Net.BCrypt.HashPassword(pin),
                AccountId = account.Id,
                CreatedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }

        private async Task<string> LoginAndGetTokenAsync(HttpClient client, string cardNumber, string pin)
        {
            var loginRes = await client.PostAsJsonAsync("/api/auth/login", new { CardNumber = cardNumber, Pin = pin });
            if (loginRes.StatusCode != HttpStatusCode.OK)
            {
                var body = await loginRes.Content.ReadAsStringAsync();
                throw new Exception($"Login status={loginRes.StatusCode} body={body}");
            }

            var json = await loginRes.Content.ReadAsStringAsync();
            // AuthController devuelve { token = "..." } (propiedad minúscula)
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("token").GetString();
            if (string.IsNullOrWhiteSpace(token)) throw new Exception($"Empty token. Raw={json}");
            return token;
        }

        [Fact]
        public async Task FullFlow_LoginAndUseEndpoints()
        {
            await SeedAsync("4111111111111111", "1234");

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var token = await LoginAndGetTokenAsync(client, "4111111111111111", "1234");
            // Sanity: token should be a JWT (three parts)
            Assert.Equal(3, token.Split('.').Length);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var bal = await client.GetAsync("/api/account/balance/4111111111111111");
            if (bal.StatusCode != HttpStatusCode.OK)
            {
                var body = await bal.Content.ReadAsStringAsync();
                var www = bal.Headers.WwwAuthenticate.ToString();
                throw new Exception($"Balance status={bal.StatusCode} body={body} www-auth={www} token={token}");
            }

            var w = await client.PostAsJsonAsync("/api/withdraw", new WithdrawRequest("4111111111111111", 100m));
            Assert.Equal(HttpStatusCode.OK, w.StatusCode);

            var ops = await client.GetAsync("/api/operations/4111111111111111?page=1");
            Assert.Equal(HttpStatusCode.OK, ops.StatusCode);
        }

        [Fact]
        public async Task Operations_InvalidPage_ReturnsBadRequest()
        {
            await SeedAsync("4333333333333333", "1234");

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var token = await LoginAndGetTokenAsync(client, "4333333333333333", "1234");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/api/operations/4333333333333333?page=0");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Withdraw_NegativeAmount_ReturnsBadRequest()
        {
            await SeedAsync("4444444444444444", "1234");

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var token = await LoginAndGetTokenAsync(client, "4444444444444444", "1234");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsJsonAsync("/api/withdraw", new WithdrawRequest("4444444444444444", -50m));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_Blocking_AfterFourFailedAttempts()
        {
            await SeedAsync("4222222222222222", "9999");

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            for (int i = 0; i < 4; i++)
            {
                var r = await client.PostAsJsonAsync("/api/auth/login", new { CardNumber = "4222222222222222", Pin = "0000" });
                Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
            }

            var ok = await client.PostAsJsonAsync("/api/auth/login", new { CardNumber = "4222222222222222", Pin = "9999" });
            Assert.Equal(HttpStatusCode.Unauthorized, ok.StatusCode);
        }
    }
}
