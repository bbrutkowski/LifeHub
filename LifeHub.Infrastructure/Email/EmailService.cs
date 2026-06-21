using LifeHub.Application.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LifeHub.Infrastructure.Email
{
    public class EmailService(IOptions<EmailSettings> options) : IEmailService
    {
        private readonly EmailSettings _settings = options.Value;

        public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.DisplayName, _settings.From));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            //using var client = new SmtpClient();

            //await client.ConnectAsync(_settings.Host, _settings.Port,
            //    _settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable,
            //    cancellationToken);

            //await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            //await client.SendAsync(message, cancellationToken);
            //await client.DisconnectAsync(quit: true, cancellationToken);
        }
    }
}
