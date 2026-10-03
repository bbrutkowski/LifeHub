using LifeHub.Application.DTOs;
using LifeHub.Application.Interfaces.Repositories;
using LifeHub.Application.Interfaces.Services;
using LifeHub.Application.Interfaces.Utils;
using LifeHub.Domain.Entity;

namespace LifeHub.Application.Services
{
    public class UserService(
        IUserRepository userRepository,
        IRefreshTokenSessionRepository refreshTokenSessionRepository,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        IAvatarStorage avatarStorage) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IRefreshTokenSessionRepository _refreshTokenSessionRepository = refreshTokenSessionRepository;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
        private readonly IEmailService _emailService = emailService;
        private readonly IAvatarStorage _avatarStorage = avatarStorage;

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

        public async Task<UserProfileResponse?> GetProfile(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetById(userId, cancellationToken);
            return user is null ? null : ToProfile(user);
        }

        public async Task<UserProfileResponse?> UpdateProfile(Guid userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetById(userId, cancellationToken);
            if (user is null) return null;

            var email = request.Email.Trim();
            var duplicateEmail = await _userRepository.GetByEmail(email, cancellationToken);
            if (duplicateEmail is not null && duplicateEmail.Id != userId)
                throw new InvalidOperationException("This email address is already in use.");

            var name = request.Name.Trim();
            var duplicateName = await _userRepository.GetByUsername(name, cancellationToken);
            if (duplicateName is not null && duplicateName.Id != userId)
                throw new InvalidOperationException("This name is already in use.");

            user.UpdateProfile(name, email, request.Timezone.Trim(), request.City.Trim(),
                request.Currency.Trim().ToUpperInvariant(), request.DateFormat.Trim(), request.WeekStartsOn.Trim());
            await _userRepository.Update(user, cancellationToken);
            return ToProfile(user);
        }

        public async Task<bool> ChangePassword(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetById(userId, cancellationToken);
            if (user is null || !_passwordHasher.Verify(user.PasswordHash, currentPassword)) return false;

            user.UpdatePasswordHash(_passwordHasher.Hash(newPassword));
            await _userRepository.Update(user, cancellationToken);
            await _refreshTokenSessionRepository.RevokeAllForUser(userId, cancellationToken);
            return true;
        }

        public async Task<UserAvatarUploadResult> UploadAvatar(Guid userId, byte[] image, CancellationToken cancellationToken = default)
        {
            var extension = GetImageExtension(image);
            if (extension is null) return new UserAvatarUploadResult(false, null);

            var user = await _userRepository.GetById(userId, cancellationToken);
            if (user is null) return new UserAvatarUploadResult(true, null);

            var avatarUrl = await _avatarStorage.Save(image, extension, cancellationToken);
            var previousAvatar = user.AvatarUrl;
            user.UpdateAvatar(avatarUrl);
            try
            {
                await _userRepository.Update(user, cancellationToken);
            }
            catch
            {
                await _avatarStorage.Delete(avatarUrl, CancellationToken.None);
                throw;
            }

            await _avatarStorage.Delete(previousAvatar, cancellationToken);
            return new UserAvatarUploadResult(true, ToProfile(user));
        }

        public async Task<bool> DeleteAvatar(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetById(userId, cancellationToken);
            if (user is null) return false;

            var previousAvatar = user.AvatarUrl;
            user.UpdateAvatar(null);
            await _userRepository.Update(user, cancellationToken);
            await _avatarStorage.Delete(previousAvatar, cancellationToken);
            return true;
        }

        public async Task<IReadOnlyList<UserSessionResponse>> GetSessions(Guid userId, Guid currentSessionId, CancellationToken cancellationToken = default)
        {
            var sessions = await _refreshTokenSessionRepository.GetByUserId(userId, cancellationToken);
            return sessions.Select(session => new UserSessionResponse(
                session.Id,
                session.CreatedAt,
                session.ExpiresAt,
                session.IsActive,
                session.Id == currentSessionId)).ToList();
        }

        public async Task<bool> RevokeSession(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
        {
            var session = await _refreshTokenSessionRepository.GetById(sessionId, cancellationToken);
            if (session is null || session.UserId != userId) return false;
            if (session.RevokedAt is null)
            {
                session.Revoke();
                await _refreshTokenSessionRepository.Update(session, cancellationToken);
            }

            return true;
        }

        public Task RevokeOtherSessions(Guid userId, Guid currentSessionId, CancellationToken cancellationToken = default) =>
            _refreshTokenSessionRepository.RevokeAllExceptForUser(userId, currentSessionId, cancellationToken);

        public async Task<UserAccountExportResponse?> GetAccountExport(Guid userId, CancellationToken cancellationToken = default)
        {
            var profile = await GetProfile(userId, cancellationToken);
            if (profile is null) return null;

            var sessions = await _refreshTokenSessionRepository.GetByUserId(userId, cancellationToken);
            return new UserAccountExportResponse(
                profile,
                sessions.Select(session => new UserAccountExportSessionResponse(
                    session.CreatedAt,
                    session.ExpiresAt,
                    session.RevokedAt)).ToList(),
                DateTimeOffset.UtcNow);
        }

        public async Task<UserProfileResponse?> DeactivateAccount(Guid userId, string currentPassword, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetById(userId, cancellationToken);
            if (user is null || !_passwordHasher.Verify(user.PasswordHash, currentPassword)) return null;

            var profile = ToProfile(user);
            user.Deactivate();
            await _userRepository.Update(user, cancellationToken);
            await _refreshTokenSessionRepository.RevokeAllForUser(userId, cancellationToken);
            return profile;
        }

        private static UserProfileResponse ToProfile(User user) =>
            new(
                user.Id,
                user.Username,
                user.Email,
                user.AvatarUrl,
                user.Timezone ?? "Europe/Warsaw",
                user.City ?? "Warsaw",
                user.Currency ?? "PLN",
                user.DateFormat ?? "DD.MM.YYYY",
                user.WeekStartsOn ?? "monday",
                user.CreatedAt);

        private static string? GetImageExtension(byte[] bytes)
        {
            if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) return ".jpg";
            if (bytes.Length >= 8 && bytes[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return ".png";
            if (bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8.ToArray()) && bytes[8..12].SequenceEqual("WEBP"u8.ToArray())) return ".webp";
            return null;
        }
    }
}