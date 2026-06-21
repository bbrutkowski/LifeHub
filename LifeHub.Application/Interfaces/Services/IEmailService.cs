namespace LifeHub.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    }
}
