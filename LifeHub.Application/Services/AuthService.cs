using LifeHub.Application.DTOs;
using LifeHub.Application.Interfaces.Repositories;
using LifeHub.Application.Interfaces.Services;
using LifeHub.Application.Interfaces.Utils;
using LifeHub.Domain.Entity;

namespace LifeHub.Application.Services
{
    public class AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRefreshTokenSessionRepository refreshTokenSessionRepository) : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IRefreshTokenSessionRepository _refreshTokenSessionRepository = refreshTokenSessionRepository;

        public async Task<LoginResponse?> Authenticate(LoginRequest loginRequest, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmail(loginRequest.Email, cancellationToken);
            if (user is null) return null;

            var verified = _passwordHasher.Verify(user.PasswordHash, loginRequest.Password);
            if (!verified) return null;

            var (token, expiresAt) = await _tokenService.GenerateToken(user);
            var (refreshToken, refreshTokenExpiresAt) = _tokenService.GenerateRefreshToken();
            var refreshTokenHash = _tokenService.HashRefreshToken(refreshToken);

            await _refreshTokenSessionRepository.Add(
                new RefreshTokenSession(Guid.NewGuid(), user.Id, refreshTokenHash, refreshTokenExpiresAt),
                cancellationToken);

            return new LoginResponse(token, refreshToken, expiresAt, user.Id, user.Username, user.Email);
        }

        public async Task<RefreshTokenResponse?> RefreshAccessToken(RefreshTokenRequest refreshTokenRequest, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenRequest.RefreshToken)) return null;

            var currentTokenHash = _tokenService.HashRefreshToken(refreshTokenRequest.RefreshToken);
            var session = await _refreshTokenSessionRepository.GetByTokenHash(currentTokenHash, cancellationToken);
            if (session is null || !session.IsActive)
            {
                return null;
            }

            var user = await _userRepository.GetById(session.UserId, cancellationToken);
            if (user is null)
            {
                return null;
            }

            var (token, expiresAt) = await _tokenService.GenerateToken(user);
            var (nextRefreshToken, nextRefreshTokenExpiresAt) = _tokenService.GenerateRefreshToken();
            var nextTokenHash = _tokenService.HashRefreshToken(nextRefreshToken);

            session.Revoke(nextTokenHash);
            await _refreshTokenSessionRepository.Update(session, cancellationToken);

            await _refreshTokenSessionRepository.Add(
                new RefreshTokenSession(Guid.NewGuid(), user.Id, nextTokenHash, nextRefreshTokenExpiresAt),
                cancellationToken);

            return new RefreshTokenResponse(token, nextRefreshToken, expiresAt);
        }
    }
}
