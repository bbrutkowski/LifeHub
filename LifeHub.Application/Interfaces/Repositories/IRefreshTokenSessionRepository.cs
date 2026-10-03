using LifeHub.Domain.Entity;

namespace LifeHub.Application.Interfaces.Repositories
{
    public interface IRefreshTokenSessionRepository
    {
        Task<RefreshTokenSession?> GetByTokenHash(string tokenHash, CancellationToken cancellationToken = default);
        Task<RefreshTokenSession?> GetById(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<RefreshTokenSession>> GetByUserId(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> IsNotRevoked(Guid id, Guid userId, CancellationToken cancellationToken = default);
        Task Add(RefreshTokenSession session, CancellationToken cancellationToken = default);
        Task Update(RefreshTokenSession session, CancellationToken cancellationToken = default);
        Task RevokeAllForUser(Guid userId, CancellationToken cancellationToken = default);
        Task RevokeAllExceptForUser(Guid userId, Guid exceptSessionId, CancellationToken cancellationToken = default);
    }
}
