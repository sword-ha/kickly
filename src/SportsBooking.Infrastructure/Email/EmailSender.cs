using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SportsBooking.Application.Interfaces;
using SportsBooking.Application.Options;

namespace SportsBooking.Infrastructure.Email;

public sealed class EmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<SmtpOptions> options, ILogger<EmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.FromAddress))
        {
            throw new InvalidOperationException(
                "SMTP is not configured. Set the 'Smtp' section in appsettings.json before sending emails.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(new MailboxAddress(to, to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.StartTls, ct);

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                await client.AuthenticateAsync(_options.Username, _options.Password, ct);
            }

            await client.SendAsync(message, ct);
            _logger.LogInformation("Email '{Subject}' sent to {To}.", subject, to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email '{Subject}' to {To}.", subject, to);
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true, ct);
        }
    }
}
