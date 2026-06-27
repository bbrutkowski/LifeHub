using LifeHub.Application.DTOs;
using LifeHub.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LifeHub.Api.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController(IUserService userService, IAuthService authService) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly IAuthService _authService = authService;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request, CancellationToken cancellationToken)
        {
            await _userService.Add(request, cancellationToken);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest, CancellationToken cancellationToken)
        {
            var authResult = await _authService.Authenticate(loginRequest, cancellationToken);
            if (authResult is null) return Unauthorized(new { message = "Invalid credentials" });

            return Ok(authResult);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var authResult = await _authService.RefreshAccessToken(request, cancellationToken);
            if (authResult is null) return Unauthorized(new { message = "Invalid refresh token" });

            return Ok(authResult);
        }

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            await _userService.SendPasswordResetEmail(request.Email, cancellationToken);
            return Ok();
        }
    }
}
