using LifeHub.Application.DTOs;
using LifeHub.Application.Interfaces;

namespace LifeHub.Application.Services
{
    public class AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService) : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
        private readonly ITokenService _tokenService = tokenService;

        public async Task<LoginResponse?> Authenticate(LoginRequest loginRequest, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmail(loginRequest.Email, cancellationToken);
            if (user is null) return null;

            var verified = _passwordHasher.Verify(user.PasswordHash, loginRequest.Password);
            if (!verified) return null;

            var (token, expiresAt) = await _tokenService.GenerateToken(user);

            return new LoginResponse(token, expiresAt, user.Id, user.Username, user.Email);
        }
    }
}
