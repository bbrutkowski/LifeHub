namespace LifeHub.Domain.Entity
{
    public class RefreshTokenSession
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string TokenHash { get; private set; } = null!;
        public DateTimeOffset ExpiresAt { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? RevokedAt { get; private set; }
        public string? ReplacedByTokenHash { get; private set; }

        public User User { get; private set; } = null!;

        private RefreshTokenSession() { }

        public RefreshTokenSession(Guid id, Guid userId, string tokenHash, DateTimeOffset expiresAt, DateTimeOffset? createdAt = null)
        {
            Id = id;
            UserId = userId;
            TokenHash = tokenHash;
            ExpiresAt = expiresAt;
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
        }

        public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;

        public void Revoke(string? replacedByTokenHash = null)
        {
            RevokedAt = DateTimeOffset.UtcNow;
            ReplacedByTokenHash = replacedByTokenHash;
        }
    }
}
