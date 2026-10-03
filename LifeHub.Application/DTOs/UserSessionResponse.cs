namespace LifeHub.Application.DTOs;

public sealed record UserSessionResponse(
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    bool IsActive,
    bool IsCurrent);
