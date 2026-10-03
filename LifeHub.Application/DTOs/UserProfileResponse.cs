namespace LifeHub.Application.DTOs;

public record UserProfileResponse(
    Guid Id,
    string Name,
    string Email,
    string? AvatarUrl,
    string Timezone,
    string City,
    string Currency,
    string DateFormat,
    string WeekStartsOn,
    DateTimeOffset CreatedAt);
