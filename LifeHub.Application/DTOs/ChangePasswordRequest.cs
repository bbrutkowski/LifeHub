using System.ComponentModel.DataAnnotations;

namespace LifeHub.Application.DTOs;

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, MinLength(8)] string NewPassword);
