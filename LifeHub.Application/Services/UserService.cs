using LifeHub.Application.DTOs;
using LifeHub.Application.Interfaces.Repositories;
using LifeHub.Application.Interfaces.Services;
using LifeHub.Application.Interfaces.Utils;
using LifeHub.Domain.Entity;

namespace LifeHub.Application.Services
{
    public class UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, IEmailService emailService) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
        private readonly IEmailService _emailService = emailService;

        public async Task Add(UserRegisterRequest request, CancellationToken cancellationToken = default)
        {
            var user = new User(Guid.NewGuid(), request.Name, request.Email, _passwordHasher.Hash(request.Password));

            await _userRepository.Add(user, cancellationToken);
        }

        public async Task SendPasswordResetEmail(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmail(email, cancellationToken);
            if (user is null) return;

            var resetToken = Guid.NewGuid().ToString("N");
            var resetLink = $"https://lifehub.app/reset-password?token={resetToken}&email={Uri.EscapeDataString(email)}";

            var subject = "Reset password – LifeHub";
            var body = $"""
                <p>We have received a request to reset the password for the account associated with the address <strong>{email}</strong>.</p>
                <p>Click the link below to set a new password (link is valid for 24 hours):</p>
                <p><a href="{resetLink}">{resetLink}</a></p>
                """;

            await _emailService.SendAsync(email, subject, body, cancellationToken);
        }
    }
}
