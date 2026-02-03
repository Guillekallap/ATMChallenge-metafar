using ATMChallenge.Application.Features.Account;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMChallenge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet("balance/{cardNumber}")]
        public async Task<IActionResult> GetBalance(string cardNumber)
        {
            // Asegurar que el token corresponde a la tarjeta solicitada.
            var tokenCard = User.FindFirst("cardNumber")?.Value;
            if (string.IsNullOrWhiteSpace(tokenCard) || tokenCard != cardNumber)
            {
                return Forbid();
            }

            try
            {
                var result = await _mediator.Send(new GetBalanceQuery(cardNumber));
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { code = 404, message = "Tarjeta no encontrada" });
            }
        }
    }
}
