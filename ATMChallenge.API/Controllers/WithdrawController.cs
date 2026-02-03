using ATMChallenge.Application.DTOs.Requests;
using ATMChallenge.Application.Features.Account;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMChallenge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WithdrawController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WithdrawController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
        {
            var tokenCard = User.FindFirst("cardNumber")?.Value;
            if (string.IsNullOrWhiteSpace(tokenCard) || tokenCard != request.CardNumber)
            {
                return Forbid();
            }

            var result = await _mediator.Send(new WithdrawCommand(request.CardNumber, request.Amount));
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(new { newBalance = result.NewBalance, timestamp = result.Timestamp });
        }
    }
}
