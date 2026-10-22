using Microsoft.Extensions.Logging;

namespace PotionCraft.Server.Services.Email;

/// <summary>
/// Заглушка отправителя писем: записывает письмо в лог сервера вместо реальной отправки.
/// Будет заменена реализацией через SMTP.
/// </summary>
public class ConsoleEmailSender : IEmailSender
{
    /// <summary>Логгер, в который записываются письма.</summary>
    private readonly ILogger<ConsoleEmailSender> _logger;

    /// <summary>
    /// Создаёт заглушку отправителя писем.
    /// </summary>
    /// <param name="logger">Логгер.</param>
    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Записывает письмо в лог уровня Information.
    /// </summary>
    /// <param name="to">Адрес получателя.</param>
    /// <param name="subject">Тема письма.</param>
    /// <param name="body">Текст письма.</param>
    /// <param name="ct">Токен отмены операции (не используется).</param>
    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        _logger.LogInformation("[EMAIL-ЗАГЛУШКА] Кому: {To}; Тема: {Subject}; {Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
