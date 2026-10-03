using LifeHub.Application.DTOs;

namespace LifeHub.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task Add(UserRegisterRequest request, CancellationToken cancellationToken = default);
        Task SendPasswordResetEmail(string email, CancellationToken cancellationToken = default);
        Task<UserProfileResponse?> GetProfile(Guid userId, CancellationToken cancellationToken = default);
        Task<UserProfileResponse?> UpdateProfile(Guid userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
        Task<bool> ChangePassword(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
        Task<UserAvatarUploadResult> UploadAvatar(Guid userId, byte[] image, CancellationToken cancellationToken = default);
        Task<bool> DeleteAvatar(Guid userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<UserSessionResponse>> GetSessions(Guid userId, Guid currentSessionId, CancellationToken cancellationToken = default);
        Task<bool> RevokeSession(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);
        Task RevokeOtherSessions(Guid userId, Guid currentSessionId, CancellationToken cancellationToken = default);
        Task<UserAccountExportResponse?> GetAccountExport(Guid userId, CancellationToken cancellationToken = default);
        Task<UserProfileResponse?> DeactivateAccount(Guid userId, string currentPassword, CancellationToken cancellationToken = default);
    }
}
