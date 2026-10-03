namespace LifeHub.Application.DTOs;

public sealed record UserAccountExportResponse(
    UserProfileResponse Profile,
    IReadOnlyList<UserAccountExportSessionResponse> Sessions,
    DateTimeOffset ExportedAt);

public sealed record UserAccountExportSessionResponse(
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt);
