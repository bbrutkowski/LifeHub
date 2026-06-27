using LifeHub.Application.Interfaces.Repositories;
using LifeHub.Domain.Entity;
using LifeHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LifeHub.Infrastructure.Repository
{
    public class RefreshTokenSessionRepository(ApplicationDbContext db) : IRefreshTokenSessionRepository
    {
        private readonly ApplicationDbContext _db = db;

        public async Task<RefreshTokenSession?> GetByTokenHash(string tokenHash, CancellationToken cancellationToken = default)
        {
            return await _db.Set<RefreshTokenSession>()
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
        }

        public async Task Add(RefreshTokenSession session, CancellationToken cancellationToken = default)
        {
            await _db.Set<RefreshTokenSession>().AddAsync(session, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task Update(RefreshTokenSession session, CancellationToken cancellationToken = default)
        {
            _db.Set<RefreshTokenSession>().Update(session);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task RevokeAllForUser(Guid userId, CancellationToken cancellationToken = default)
        {
            var activeSessions = await _db.Set<RefreshTokenSession>()
                .Where(x => x.UserId == userId && x.RevokedAt == null)
                .ToListAsync(cancellationToken);

            if (activeSessions.Count == 0)
            {
                return;
            }

            foreach (var session in activeSessions)
            {
                session.Revoke();
            }

            _db.Set<RefreshTokenSession>().UpdateRange(activeSessions);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
