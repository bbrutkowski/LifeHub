using LifeHub.Application.DTOs;
using LifeHub.Domain.Entity;

namespace LifeHub.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetById(Guid id, CancellationToken cancellationToken = default);
        Task<User?> GetByUsername(string username, CancellationToken cancellationToken = default);
        Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default);
        Task Add(User user, CancellationToken cancellationToken = default);
        Task Update(User user, CancellationToken cancellationToken = default);
    }
}
