using LifeHub.Application.DTOs;

namespace LifeHub.Application.Interfaces
{
    public interface IUserService
    {
        Task Add(UserRegisterRequest request, CancellationToken cancellationToken = default);
    }
}
