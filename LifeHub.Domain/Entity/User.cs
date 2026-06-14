namespace LifeHub.Domain.Entity
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Username { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public DateTimeOffset CreatedAt { get; private set; }

        private User() { }

        public User(Guid id, string username, string email, string passwordHash, DateTimeOffset? createdAt = null)
        {
            Id = id;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
        }

        public void UpdatePasswordHash(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
        }

        public void UpdateEmail(string email)
        {
            Email = email;
        }
    }
}
