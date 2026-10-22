using MimeKit;

namespace PotionCraft.Server.Services.Email;

/// <summary>
/// Настройки отправки электронной почты, читаемые из секции "Email" конфигурации.
/// </summary>
public class EmailOptions
{
    /// <summary>Название секции конфигурации с настройками почты.</summary>
    public const string SectionName = "Email";

    /// <summary>Провайдер, который только выводит письма в лог (для разработки).</summary>
    public const string ConsoleProvider = "Console";

    /// <summary>Провайдер, который отправляет письма через SMTP-сервер.</summary>
    public const string SmtpProvider = "Smtp";

    /// <summary>Порт SMTP по умолчанию (submission с STARTTLS).</summary>
    public const int DefaultSmtpPort = 587;

    /// <summary>Таймаут SMTP-операций в секундах по умолчанию.</summary>
    public const int DefaultTimeoutSeconds = 30;

    /// <summary>Провайдер отправки: "Console" или "Smtp" (регистр не важен).</summary>
    public string Provider { get; set; } = ConsoleProvider;

    /// <summary>Адрес отправителя, например "PotionCraft &lt;noreply@example.com&gt;".</summary>
    public string From { get; set; } = string.Empty;

    /// <summary>Настройки SMTP-сервера.</summary>
    public SmtpOptions Smtp { get; set; } = new();

    /// <summary>
    /// Проверяет корректность настроек и возвращает список ошибок на русском языке.
    /// </summary>
    /// <returns>Список сообщений об ошибках; пустой, если настройки корректны.</returns>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        var isConsole = string.Equals(Provider, ConsoleProvider, StringComparison.OrdinalIgnoreCase);
        var isSmtp = string.Equals(Provider, SmtpProvider, StringComparison.OrdinalIgnoreCase);

        if (!isConsole && !isSmtp)
        {
            errors.Add($"Неизвестный провайдер почты \"{Provider}\". Допустимые значения: {ConsoleProvider}, {SmtpProvider}.");
            return errors;
        }

        if (!isSmtp)
            return errors;

        if (string.IsNullOrWhiteSpace(From))
            errors.Add("Не задан адрес отправителя (Email:From).");
        else if (!MailboxAddress.TryParse(From, out _))
            errors.Add($"Адрес отправителя \"{From}\" имеет неверный формат (Email:From).");

        if (string.IsNullOrWhiteSpace(Smtp.Host))
            errors.Add("Не задан адрес SMTP-сервера (Email:Smtp:Host).");

        if (Smtp.Port is < 1 or > 65535)
            errors.Add($"Порт SMTP {Smtp.Port} вне допустимого диапазона 1–65535 (Email:Smtp:Port).");

        if (Smtp.TimeoutSeconds <= 0)
            errors.Add("Таймаут SMTP должен быть больше нуля (Email:Smtp:TimeoutSeconds).");

        return errors;
    }
}

/// <summary>
/// Настройки подключения к SMTP-серверу.
/// </summary>
public class SmtpOptions
{
    /// <summary>Адрес SMTP-сервера.</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>Порт SMTP-сервера.</summary>
    public int Port { get; set; } = EmailOptions.DefaultSmtpPort;

    /// <summary>Имя пользователя для авторизации; если пусто, авторизация не выполняется.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Пароль для авторизации на SMTP-сервере.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Использовать STARTTLS; если выключено, соединение выбирается автоматически (например, SSL на порту 465).</summary>
    public bool UseStartTls { get; set; } = true;

    /// <summary>Таймаут SMTP-операций в секундах.</summary>
    public int TimeoutSeconds { get; set; } = EmailOptions.DefaultTimeoutSeconds;
}
