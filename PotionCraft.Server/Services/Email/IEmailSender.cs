namespace PotionCraft.Server.Services.Email;

/// <summary>
/// Отправитель электронных писем приложения.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Отправляет письмо указанному получателю.
    /// </summary>
    /// <param name="to">Адрес получателя.</param>
    /// <param name="subject">Тема письма.</param>
    /// <param name="body">Текст письма.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}
