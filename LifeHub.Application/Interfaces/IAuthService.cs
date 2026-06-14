using LifeHub.Application.DTOs;

namespace LifeHub.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse?> Authenticate(LoginRequest loginRequest, CancellationToken cancellationToken = default);
    }
}
