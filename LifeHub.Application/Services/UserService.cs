using LifeHub.Application.DTOs;
using LifeHub.Application.Interfaces;
using LifeHub.Domain.Entity;

namespace LifeHub.Application.Services
{
    public class UserService(IUserRepository userRepository, IPasswordHasher passwordHasher) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;

        public async Task Add(UserRegisterRequest request, CancellationToken cancellationToken = default)
        {
            var user = new User(Guid.NewGuid(), request.Name, request.Email, _passwordHasher.Hash(request.Password));

            await _userRepository.Add(user, cancellationToken);
        }
    }
}
