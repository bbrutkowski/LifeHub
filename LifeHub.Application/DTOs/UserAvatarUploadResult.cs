namespace LifeHub.Application.DTOs;

public sealed record UserAvatarUploadResult(
    bool IsValidImage,
    UserProfileResponse? Profile);
