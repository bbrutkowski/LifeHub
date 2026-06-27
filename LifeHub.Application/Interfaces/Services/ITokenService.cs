using LifeHub.Domain.Entity;

namespace LifeHub.Application.Interfaces.Services
{
    public interface ITokenService
    {
        Task<(string Token, DateTime ExpiresAt)> GenerateToken(User user);
        (string RefreshToken, DateTimeOffset ExpiresAt) GenerateRefreshToken();
        string HashRefreshToken(string refreshToken);
    }
}
