namespace LifeHub.Application.DTOs
{
    public record RefreshTokenResponse(string Token, string RefreshToken, DateTime ExpiresAt);
}
