using LifeHub.Application.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;

namespace LifeHub.Infrastructure.Storage;

public sealed class AvatarStorage(IWebHostEnvironment environment) : IAvatarStorage
{
    private const string AvatarUrlPrefix = "/uploads/avatars/";

    public async Task<string> Save(byte[] image, string extension, CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(environment.ContentRootPath, "wwwroot", "uploads", "avatars");
        Directory.CreateDirectory(directory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var destination = Path.Combine(directory, fileName);
        await File.WriteAllBytesAsync(destination, image, cancellationToken);
        return $"{AvatarUrlPrefix}{fileName}";
    }

    public Task Delete(string? avatarUrl, CancellationToken cancellationToken = default)
    {
        if (avatarUrl is null || !avatarUrl.StartsWith(AvatarUrlPrefix, StringComparison.Ordinal))
            return Task.CompletedTask;

        var fileName = Path.GetFileName(avatarUrl);
        var path = Path.Combine(environment.ContentRootPath, "wwwroot", "uploads", "avatars", fileName);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }
}
