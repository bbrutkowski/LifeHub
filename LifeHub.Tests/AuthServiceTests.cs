using LifeHub.Application.DTOs;
using LifeHub.Application.Interfaces.Repositories;
using LifeHub.Application.Interfaces.Services;
using LifeHub.Application.Interfaces.Utils;
using LifeHub.Application.Services;
using LifeHub.Domain.Entity;

namespace LifeHub.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task Authenticate_WithValidCredentials_ReturnsTokensAndCreatesRefreshSession()
    {
        var user = new User(Guid.NewGuid(), "john", "john@lifehub.local", "hashed-password");
        var userRepository = new FakeUserRepository(user);
        var refreshRepository = new FakeRefreshTokenSessionRepository();
        var passwordHasher = new FakePasswordHasher(verifyResult: true);
        var tokenService = new FakeTokenService(
            accessToken: "access-token",
            refreshTokens: new Queue<(string RefreshToken, DateTimeOffset ExpiresAt)>(
                [
                    ("refresh-token", DateTimeOffset.UtcNow.AddDays(10))
                ]));

        var sut = new AuthService(userRepository, passwordHasher, tokenService, refreshRepository);

        var result = await sut.Authenticate(new LoginRequest(user.Email, "secret"));

        Assert.NotNull(result);
        Assert.Equal("access-token", result!.Token);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Single(refreshRepository.AddedSessions);
        Assert.Equal(user.Id, refreshRepository.AddedSessions[0].UserId);
        Assert.Equal("hash-refresh-token", refreshRepository.AddedSessions[0].TokenHash);
    }

    [Fact]
    public async Task RefreshAccessToken_WithActiveSession_RotatesSessionAndReturnsNewTokens()
    {
        var user = new User(Guid.NewGuid(), "kate", "kate@lifehub.local", "hashed-password");
        var session = new RefreshTokenSession(
            Guid.NewGuid(),
            user.Id,
            "hash-old-refresh",
            DateTimeOffset.UtcNow.AddDays(5));

        var userRepository = new FakeUserRepository(user);
        var refreshRepository = new FakeRefreshTokenSessionRepository(session);
        var passwordHasher = new FakePasswordHasher(verifyResult: true);
        var tokenService = new FakeTokenService(
            accessToken: "next-access-token",
            refreshTokens: new Queue<(string RefreshToken, DateTimeOffset ExpiresAt)>(
                [
                    ("next-refresh-token", DateTimeOffset.UtcNow.AddDays(20))
                ]));

        var sut = new AuthService(userRepository, passwordHasher, tokenService, refreshRepository);

        var result = await sut.RefreshAccessToken(new RefreshTokenRequest("old-refresh"));

        Assert.NotNull(result);
        Assert.Equal("next-access-token", result!.Token);
        Assert.Equal("next-refresh-token", result.RefreshToken);

        Assert.Single(refreshRepository.UpdatedSessions);
        Assert.Equal("hash-next-refresh-token", refreshRepository.UpdatedSessions[0].ReplacedByTokenHash);
        Assert.NotNull(refreshRepository.UpdatedSessions[0].RevokedAt);

        Assert.Single(refreshRepository.AddedSessions);
        Assert.Equal("hash-next-refresh-token", refreshRepository.AddedSessions[0].TokenHash);
        Assert.Equal(user.Id, refreshRepository.AddedSessions[0].UserId);
    }

    [Fact]
    public async Task RefreshAccessToken_WithInvalidToken_ReturnsNull()
    {
        var user = new User(Guid.NewGuid(), "mark", "mark@lifehub.local", "hashed-password");
        var userRepository = new FakeUserRepository(user);
        var refreshRepository = new FakeRefreshTokenSessionRepository();
        var passwordHasher = new FakePasswordHasher(verifyResult: true);
        var tokenService = new FakeTokenService(
            accessToken: "unused",
            refreshTokens: new Queue<(string RefreshToken, DateTimeOffset ExpiresAt)>());

        var sut = new AuthService(userRepository, passwordHasher, tokenService, refreshRepository);

        var result = await sut.RefreshAccessToken(new RefreshTokenRequest("unknown-refresh"));

        Assert.Null(result);
        Assert.Empty(refreshRepository.UpdatedSessions);
        Assert.Empty(refreshRepository.AddedSessions);
    }

    private sealed class FakeUserRepository(User? seededUser) : IUserRepository
    {
        private readonly User? _seededUser = seededUser;

        public Task<User?> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_seededUser?.Id == id ? _seededUser : null);
        }

        public Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_seededUser?.Email == email ? _seededUser : null);
        }

        public Task Add(User user, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task Update(User user, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeRefreshTokenSessionRepository(RefreshTokenSession? session = null) : IRefreshTokenSessionRepository
    {
        private readonly RefreshTokenSession? _session = session;

        public List<RefreshTokenSession> AddedSessions { get; } = [];
        public List<RefreshTokenSession> UpdatedSessions { get; } = [];

        public Task<RefreshTokenSession?> GetByTokenHash(string tokenHash, CancellationToken cancellationToken = default)
        {
            if (_session is null)
            {
                return Task.FromResult<RefreshTokenSession?>(null);
            }

            return Task.FromResult(_session.TokenHash == tokenHash ? _session : null);
        }

        public Task Add(RefreshTokenSession session, CancellationToken cancellationToken = default)
        {
            AddedSessions.Add(session);
            return Task.CompletedTask;
        }

        public Task Update(RefreshTokenSession session, CancellationToken cancellationToken = default)
        {
            UpdatedSessions.Add(session);
            return Task.CompletedTask;
        }

        public Task RevokeAllForUser(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordHasher(bool verifyResult) : IPasswordHasher
    {
        public string Hash(string password) => password;

        public bool Verify(string hashedPassword, string password) => verifyResult;
    }

    private sealed class FakeTokenService(string accessToken, Queue<(string RefreshToken, DateTimeOffset ExpiresAt)> refreshTokens) : ITokenService
    {
        public Task<(string Token, DateTime ExpiresAt)> GenerateToken(User user)
        {
            return Task.FromResult((accessToken, DateTime.UtcNow.AddMinutes(60)));
        }

        public (string RefreshToken, DateTimeOffset ExpiresAt) GenerateRefreshToken()
        {
            return refreshTokens.Dequeue();
        }

        public string HashRefreshToken(string refreshToken)
        {
            return $"hash-{refreshToken}";
        }
    }
}
