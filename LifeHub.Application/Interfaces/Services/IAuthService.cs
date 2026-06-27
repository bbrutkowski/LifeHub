using LifeHub.Application.DTOs;

namespace LifeHub.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> Authenticate(LoginRequest loginRequest, CancellationToken cancellationToken = default);
        Task<RefreshTokenResponse?> RefreshAccessToken(RefreshTokenRequest refreshTokenRequest, CancellationToken cancellationToken = default);
    }
}
