using LifeHub.Application.DTOs;
using LifeHub.Application.Interfaces.Repositories;
using LifeHub.Application.Interfaces.Services;
using LifeHub.Application.Interfaces.Utils;
using LifeHub.Domain.Entity;
using Microsoft.Extensions.Logging;

namespace LifeHub.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IRefreshTokenSessionRepository refreshTokenSessionRepository,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IRefreshTokenSessionRepository _refreshTokenSessionRepository = refreshTokenSessionRepository;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<LoginResponse?> Authenticate(LoginRequest loginRequest, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmail(loginRequest.Email, cancellationToken);
        if (user is null) return null;

        var verified = _passwordHasher.Verify(user.PasswordHash, loginRequest.Password);
        if (!verified) return null;

        var sessionId = Guid.NewGuid();
        var (token, expiresAt) = await _tokenService.GenerateToken(user, sessionId);
        var (refreshToken, refreshTokenExpiresAt) = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = _tokenService.HashRefreshToken(refreshToken);

        await _refreshTokenSessionRepository.Add(
            new RefreshTokenSession(sessionId, user.Id, refreshTokenHash, refreshTokenExpiresAt),
            cancellationToken);

        return new LoginResponse(token, refreshToken, expiresAt, user.Id, user.Username, user.Email);
    }

    public async Task<RefreshTokenResponse?> RefreshAccessToken(RefreshTokenRequest refreshTokenRequest, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenRequest.RefreshToken)) return null;

        var currentTokenHash = _tokenService.HashRefreshToken(refreshTokenRequest.RefreshToken);
        var session = await _refreshTokenSessionRepository.GetByTokenHash(currentTokenHash, cancellationToken);
        if (session is null || !session.IsActive) return null;

        var user = await _userRepository.GetById(session.UserId, cancellationToken);
        if (user is null) return null;

        var nextSessionId = Guid.NewGuid();
        var (token, expiresAt) = await _tokenService.GenerateToken(user, nextSessionId);
        var (nextRefreshToken, nextRefreshTokenExpiresAt) = _tokenService.GenerateRefreshToken();
        var nextTokenHash = _tokenService.HashRefreshToken(nextRefreshToken);

        session.Revoke(nextTokenHash);
        await _refreshTokenSessionRepository.Update(session, cancellationToken);

        await _refreshTokenSessionRepository.Add(
            new RefreshTokenSession(nextSessionId, user.Id, nextTokenHash, nextRefreshTokenExpiresAt),
            cancellationToken);

        _logger.LogInformation("Refresh token for user {UserId} has been refreshed. New refresh token expires at {ExpiresAt}.", user.Id, nextRefreshTokenExpiresAt);

        return new RefreshTokenResponse(token, nextRefreshToken, expiresAt);
    }
}
