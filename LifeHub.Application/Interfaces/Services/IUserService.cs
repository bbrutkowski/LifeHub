using LifeHub.Application.DTOs;

namespace LifeHub.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task Add(UserRegisterRequest request, CancellationToken cancellationToken = default);
        Task SendPasswordResetEmail(string email, CancellationToken cancellationToken = default);
    }
}
