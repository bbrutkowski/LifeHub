namespace LifeHub.Application.Interfaces.Services;

public interface IAvatarStorage
{
    Task<string> Save(byte[] image, string extension, CancellationToken cancellationToken = default);
    Task Delete(string? avatarUrl, CancellationToken cancellationToken = default);
}
