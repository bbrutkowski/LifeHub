using LifeHub.Domain.Entity;

namespace LifeHub.Application.Interfaces
{
    public interface ITokenService
    {
        Task<(string Token, DateTime ExpiresAt)> GenerateToken(User user);
    }
}
