namespace LifeHub.Application.DTOs
{
    public record LoginResponse(string Token, DateTime ExpiresAt, Guid UserId, string Username, string Email);
}
