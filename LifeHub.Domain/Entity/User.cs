namespace LifeHub.Domain.Entity
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Username { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public DateTimeOffset CreatedAt { get; private set; }
        public string? AvatarUrl { get; private set; }
        public string? Timezone { get; private set; }
        public string? City { get; private set; }
        public string? Currency { get; private set; }
        public string? DateFormat { get; private set; }
        public string? WeekStartsOn { get; private set; }
        public bool IsActive { get; private set; } = true;

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

        public void UpdateProfile(string username, string email, string timezone, string city, string currency, string dateFormat, string weekStartsOn)
        {
            Username = username;
            Email = email;
            Timezone = timezone;
            City = city;
            Currency = currency;
            DateFormat = dateFormat;
            WeekStartsOn = weekStartsOn;
        }

        public void UpdateAvatar(string? avatarUrl)
        {
            AvatarUrl = avatarUrl;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
