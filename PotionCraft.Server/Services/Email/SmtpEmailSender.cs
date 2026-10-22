using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PotionCraft.Server.Services.Email;

/// <summary>
/// Отправка писем через SMTP-сервер с помощью MailKit.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    /// <summary>Настройки отправки почты.</summary>
    private readonly EmailOptions _options;

    /// <summary>Логгер для фиксации результатов отправки.</summary>
    private readonly ILogger<SmtpEmailSender> _logger;

    /// <summary>
    /// Создаёт отправитель писем через SMTP.
    /// </summary>
    /// <param name="options">Настройки почты из конфигурации.</param>
    /// <param name="logger">Логгер для фиксации результатов отправки.</param>
    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Формирует письмо с простым текстовым телом в кодировке UTF-8.
    /// </summary>
    /// <param name="options">Настройки почты, из которых берётся адрес отправителя.</param>
    /// <param name="to">Адрес получателя.</param>
    /// <param name="subject">Тема письма.</param>
    /// <param name="body">Текст письма.</param>
    /// <returns>Готовое MIME-сообщение.</returns>
    public static MimeMessage BuildMessage(EmailOptions options, string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(options.From));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart(MimeKit.Text.TextFormat.Plain) { Text = body };
        return message;
    }

    /// <summary>
    /// Отправляет письмо через SMTP-сервер, при необходимости выполняя авторизацию.
    /// </summary>
    /// <param name="to">Адрес получателя.</param>
    /// <param name="subject">Тема письма.</param>
    /// <param name="body">Текст письма.</param>
    /// <param name="ct">Токен отмены операции.</param>
    public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        var smtp = _options.Smtp;
        var message = BuildMessage(_options, to, subject, body);
        var secureOptions = smtp.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

        try
        {
            using var client = new SmtpClient { Timeout = smtp.TimeoutSeconds * 1000 };

            await client.ConnectAsync(smtp.Host, smtp.Port, secureOptions, ct);

            if (!string.IsNullOrWhiteSpace(smtp.UserName))
                await client.AuthenticateAsync(smtp.UserName, smtp.Password, ct);

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            // Тело письма намеренно не логируется: оно может содержать код подтверждения.
            _logger.LogInformation("Письмо на адрес {Recipient} отправлено через SMTP-сервер {Host}.", to, smtp.Host);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось отправить письмо на адрес {Recipient} через SMTP-сервер {Host}.", to, smtp.Host);
            throw;
        }
    }
}
