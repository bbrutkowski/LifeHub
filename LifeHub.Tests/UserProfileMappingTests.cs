using LifeHub.Application.Interfaces.Repositories;
using LifeHub.Application.Interfaces.Services;
using LifeHub.Application.Interfaces.Utils;
using LifeHub.Application.Services;
using LifeHub.Domain.Entity;

namespace LifeHub.Tests;

public class UserProfileMappingTests
{
    [Fact]
    public async Task GetProfile_UsesDefaultsWhenStoredProfileFieldsAreNull()
    {
        var user = new User(Guid.NewGuid(), "jane", "jane@example.com", "hashed-password");
        var service = new UserService(
            new FakeUserRepository(user),
            new FakeRefreshTokenSessionRepository(),
            new FakePasswordHasher(),
            new FakeEmailService(),
            new FakeAvatarStorage());

        var profile = await service.GetProfile(user.Id);

        Assert.NotNull(profile);
        Assert.Equal("Europe/Warsaw", profile.Timezone);
        Assert.Equal("Warsaw", profile.City);
        Assert.Equal("PLN", profile.Currency);
        Assert.Equal("DD.MM.YYYY", profile.DateFormat);
        Assert.Equal("monday", profile.WeekStartsOn);
    }

    private sealed class FakeUserRepository(User user) : IUserRepository
    {
        public Task<User?> GetById(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<User?>(id == user.Id ? user : null);

        public Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult<User?>(email == user.Email ? user : null);

        public Task<User?> GetByUsername(string username, CancellationToken cancellationToken = default) =>
            Task.FromResult<User?>(username == user.Username ? user : null);

        public Task Add(User newUser, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Update(User updatedUser, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeRefreshTokenSessionRepository : IRefreshTokenSessionRepository
    {
        public Task<RefreshTokenSession?> GetByTokenHash(string tokenHash, CancellationToken cancellationToken = default) =>
            Task.FromResult<RefreshTokenSession?>(null);

        public Task<RefreshTokenSession?> GetById(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<RefreshTokenSession?>(null);

        public Task<IReadOnlyList<RefreshTokenSession>> GetByUserId(Guid userId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<RefreshTokenSession>>([]);

        public Task<bool> IsNotRevoked(Guid id, Guid userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task Add(RefreshTokenSession session, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Update(RefreshTokenSession session, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task RevokeAllForUser(Guid userId, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task RevokeAllExceptForUser(Guid userId, Guid exceptSessionId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => password;

        public bool Verify(string hashedPassword, string password) => false;
    }

    private sealed class FakeEmailService : IEmailService
    {
        public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeAvatarStorage : IAvatarStorage
    {
        public Task<string> Save(byte[] image, string extension, CancellationToken cancellationToken = default) =>
            Task.FromResult("avatar.png");

        public Task Delete(string? avatarUrl, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
