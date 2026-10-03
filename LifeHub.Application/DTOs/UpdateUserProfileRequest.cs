using System.ComponentModel.DataAnnotations;

namespace LifeHub.Application.DTOs;

public record UpdateUserProfileRequest(
    [Required, MinLength(2), MaxLength(100)] string Name,
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(100)] string Timezone,
    [Required, MinLength(2), MaxLength(100)] string City,
    [Required, StringLength(3, MinimumLength = 3)] string Currency,
    [Required, MaxLength(20)] string DateFormat,
    [Required, MaxLength(10)] string WeekStartsOn);
