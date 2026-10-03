using System.Security.Claims;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using LifeHub.Application.DTOs;
using LifeHub.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace LifeHub.Api.Controllers;

[Route("api/user")]
[ApiController]
public class UserController(
    IUserService userService,
    IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterRequest request, CancellationToken cancellationToken)
    {
        await userService.Add(request, cancellationToken);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest, CancellationToken cancellationToken)
    {
        var authResult = await authService.Authenticate(loginRequest, cancellationToken);
        if (authResult is null) return Unauthorized(new { message = "Invalid credentials" });
        return Ok(authResult);
    }

    [HttpPost("refreshToken")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var authResult = await authService.RefreshAccessToken(request, cancellationToken);
        if (authResult is null) return Unauthorized(new { message = "Invalid refresh token" });
        return Ok(authResult);
    }

    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await userService.SendPasswordResetEmail(request.Email, cancellationToken);
        return Ok();
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProfile(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId) || userId != id) return Forbid();
        var profile = await userService.GetProfile(userId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateUserProfileRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId) || userId != id) return Forbid();
        try
        {
            var profile = await userService.UpdateProfile(userId, request, cancellationToken);
            return profile is null ? NotFound() : Ok(profile);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [Authorize]
    [HttpPost("changePassword")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var changed = await userService.ChangePassword(userId, request.CurrentPassword, request.NewPassword, cancellationToken);
        return changed ? NoContent() : BadRequest(new { message = "Current password is incorrect." });
    }

    [Authorize]
    [HttpPost("avatar")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (file.Length is <= 0 or > 5 * 1024 * 1024)
            return BadRequest(new { message = "Avatar must be smaller than 5 MB." });

        await using var input = file.OpenReadStream();
        using var memory = new MemoryStream();
        await input.CopyToAsync(memory, cancellationToken);
        var bytes = memory.ToArray();
        var result = await userService.UploadAvatar(userId, bytes, cancellationToken);
        if (!result.IsValidImage)
            return BadRequest(new { message = "Upload a valid JPEG, PNG, or WebP image." });

        return result.Profile is null ? NotFound() : Ok(result.Profile);
    }

    [Authorize]
    [HttpDelete("avatar")]
    public async Task<IActionResult> DeleteAvatar(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (!await userService.DeleteAvatar(userId, cancellationToken)) return NotFound();
        return NoContent();
    }

    [Authorize]
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId) || !TryGetSessionId(out var currentSessionId)) return Unauthorized();
        var userSessions = await userService.GetSessions(userId, currentSessionId, cancellationToken);
        return Ok(userSessions);
    }

    [Authorize]
    [HttpDelete("sessions/{id:guid}")]
    public async Task<IActionResult> RevokeSession(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (!await userService.RevokeSession(userId, id, cancellationToken)) return NotFound();
        return NoContent();
    }

    [Authorize]
    [HttpDelete("sessions")]
    public async Task<IActionResult> RevokeOtherSessions(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId) || !TryGetSessionId(out var currentSessionId)) return Unauthorized();
        await userService.RevokeOtherSessions(userId, currentSessionId, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId) || !TryGetSessionId(out var sessionId)) return Unauthorized();
        await userService.RevokeSession(userId, sessionId, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("export")]
    public async Task<IActionResult> ExportData(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var export = await userService.GetAccountExport(userId, cancellationToken);
        if (export is null) return NotFound();
        var json = JsonSerializer.Serialize(export, new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true });
        return File(Encoding.UTF8.GetBytes(json), "application/json", "lifehub-account-export.json");
    }

    [Authorize]
    [HttpPost("deactivate")]
    public async Task<IActionResult> Deactivate([FromBody] ChangePasswordConfirmation request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var profile = await userService.DeactivateAccount(userId, request.CurrentPassword, cancellationToken);
        if (profile is null) return BadRequest(new { message = "Current password is incorrect." });
        return NoContent();
    }

    private bool TryGetUserId(out Guid userId) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out userId);

    private bool TryGetSessionId(out Guid sessionId) => Guid.TryParse(User.FindFirstValue("sid"), out sessionId);

    public sealed record ChangePasswordConfirmation([Required] string CurrentPassword);
}
