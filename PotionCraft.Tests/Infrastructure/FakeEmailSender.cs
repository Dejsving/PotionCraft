using System.Text.RegularExpressions;
using PotionCraft.Server.Services.Email;

namespace PotionCraft.Tests.Infrastructure;

/// <summary>
/// Отправленное письмо, сохранённое фиктивным отправителем.
/// </summary>
/// <param name="To">Адрес получателя.</param>
/// <param name="Subject">Тема письма.</param>
/// <param name="Body">Текст письма.</param>
public sealed record SentEmail(string To, string Subject, string Body);

/// <summary>
/// Фиктивный отправитель писем: сохраняет письма в памяти для проверки в тестах.
/// </summary>
public sealed class FakeEmailSender : IEmailSender
{
    /// <summary>Шаблон, по которому извлекается код подтверждения из текста письма.</summary>
    private static readonly Regex CodePattern = new(@"код подтверждения: (\d{6})", RegexOptions.Compiled);

    /// <summary>Отправленные письма в порядке отправки.</summary>
    private readonly List<SentEmail> _sent = new();

    /// <summary>Отправленные письма в порядке отправки.</summary>
    public IReadOnlyList<SentEmail> Sent => _sent;

    /// <summary>
    /// Сохраняет письмо в памяти.
    /// </summary>
    /// <param name="to">Адрес получателя.</param>
    /// <param name="subject">Тема письма.</param>
    /// <param name="body">Текст письма.</param>
    /// <param name="ct">Токен отмены операции (не используется).</param>
    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        _sent.Add(new SentEmail(to, subject, body));
        return Task.CompletedTask;
    }

    /// <summary>
    /// Возвращает код подтверждения из последнего отправленного письма.
    /// </summary>
    /// <returns>Шестизначный код.</returns>
    public string LastCode()
    {
        var match = CodePattern.Match(_sent[^1].Body);
        if (!match.Success)
            throw new InvalidOperationException("В последнем письме не найден код подтверждения.");
        return match.Groups[1].Value;
    }
}
