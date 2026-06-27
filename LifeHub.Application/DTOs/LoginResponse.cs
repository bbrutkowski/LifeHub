namespace LifeHub.Application.DTOs
{
    public record LoginResponse(string Token, string RefreshToken, DateTime ExpiresAt, Guid UserId, string Username, string Email);
}
