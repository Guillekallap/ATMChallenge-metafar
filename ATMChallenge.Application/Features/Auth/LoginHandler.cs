using ATMChallenge.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ATMChallenge.Application.Features.Auth
{
    public class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
    {
        private readonly ICardRepository _cardRepository;
        private readonly IConfiguration _config;

        public LoginHandler(ICardRepository cardRepository, IConfiguration config)
        {
            _cardRepository = cardRepository;
            _config = config;
        }

        public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var card = await _cardRepository.GetByCardNumberAsync(request.CardNumber);
            if (card == null) return new LoginResult(false, null, "Card not found");
            if (card.IsBlocked) return new LoginResult(false, null, "Card is blocked");

            // Validar PIN (hash)
            if (!BCrypt.Net.BCrypt.Verify(request.Pin, card.PinHash))
            {
                card.FailedPinAttempts++;
                if (card.FailedPinAttempts >= 4)
                {
                    card.IsBlocked = true;
                }

                await _cardRepository.UpdateAsync(card);
                return new LoginResult(false, null, "Invalid PIN");
            }

            // Resetear intentos fallidos
            card.FailedPinAttempts = 0;
            await _cardRepository.UpdateAsync(card);

            var token = GenerateToken(card.CardNumber);
            return new LoginResult(true, token, null);
        }

        private string GenerateToken(string cardNumber)
        {
            // Usar key del config; si no existe, usar un valor por defecto suficientemente largo.
            var rawKey = _config["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(rawKey))
            {
                rawKey = "ThisIsASecureLongKeyForJWT_ChangeInProduction_0123456789ABCDEF";
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(rawKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Emitimos un claim explícito para el número de tarjeta.
            // Evita depender del mapeo interno de ClaimTypes.Name -> "unique_name".
            var claims = new List<Claim>
            {
                new Claim("cardNumber", cardNumber)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "ATMChallengeIssuer",
                audience: _config["Jwt:Audience"] ?? "ATMChallengeAudience",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpireMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
