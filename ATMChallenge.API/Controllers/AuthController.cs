using ATMChallenge.Application.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ATMChallenge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Success) return Unauthorized(new { message = result.Message });
            return Ok(new { token = result.Token });
        }
    }
}
