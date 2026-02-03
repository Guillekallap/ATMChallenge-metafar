using ATMChallenge.Application.Features.Account;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMChallenge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OperationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OperationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet("{cardNumber}")]
        public async Task<IActionResult> GetOperations(string cardNumber, [FromQuery] int page = 1)
        {
            // Validación simple: page debe ser >= 1
            if (page < 1)
            {
                return BadRequest(new { code = 400, message = "El parámetro 'page' debe ser mayor o igual a 1." });
            }

            // Asegurar que el token corresponde a la tarjeta solicitada
            var tokenCard = User.FindFirst("cardNumber")?.Value;
            if (string.IsNullOrWhiteSpace(tokenCard) || tokenCard != cardNumber)
            {
                return Forbid();
            }

            var result = await _mediator.Send(new GetOperationsQuery(cardNumber, page));
            return Ok(result);
        }
    }
}
