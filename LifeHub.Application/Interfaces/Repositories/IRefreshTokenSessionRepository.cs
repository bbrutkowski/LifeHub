using LifeHub.Domain.Entity;

namespace LifeHub.Application.Interfaces.Repositories
{
    public interface IRefreshTokenSessionRepository
    {
        Task<RefreshTokenSession?> GetByTokenHash(string tokenHash, CancellationToken cancellationToken = default);
        Task Add(RefreshTokenSession session, CancellationToken cancellationToken = default);
        Task Update(RefreshTokenSession session, CancellationToken cancellationToken = default);
        Task RevokeAllForUser(Guid userId, CancellationToken cancellationToken = default);
    }
}
