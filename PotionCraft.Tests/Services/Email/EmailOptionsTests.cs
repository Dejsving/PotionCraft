using PotionCraft.Server.Services.Email;

namespace PotionCraft.Tests.Services.Email;

/// <summary>
/// Тесты для EmailOptions.Validate: проверка корректности настроек почты.
/// </summary>
public class EmailOptionsTests
{
    /// <summary>
    /// Создаёт корректные настройки SMTP для использования в тестах.
    /// </summary>
    /// <returns>Настройки с провайдером Smtp и заполненными полями.</returns>
    private static EmailOptions CreateValidSmtpOptions()
    {
        return new EmailOptions
        {
            Provider = EmailOptions.SmtpProvider,
            From = "PotionCraft <noreply@example.com>",
            Smtp = new SmtpOptions { Host = "smtp.example.com", Port = 587, TimeoutSeconds = 30 }
        };
    }

    /// <summary>
    /// Проверяет, что провайдер Console не требует настроек SMTP.
    /// </summary>
    [Fact]
    public void Validate_ConsoleProviderWithoutSmtp_ReturnsNoErrors()
    {
        var options = new EmailOptions { Provider = EmailOptions.ConsoleProvider };

        Assert.Empty(options.Validate());
    }

    /// <summary>
    /// Проверяет, что корректные настройки SMTP не дают ошибок.
    /// </summary>
    [Fact]
    public void Validate_ValidSmtpOptions_ReturnsNoErrors()
    {
        Assert.Empty(CreateValidSmtpOptions().Validate());
    }

    /// <summary>
    /// Проверяет, что пустой адрес SMTP-сервера даёт ошибку.
    /// </summary>
    [Fact]
    public void Validate_SmtpWithoutHost_ReturnsError()
    {
        var options = CreateValidSmtpOptions();
        options.Smtp.Host = "  ";

        var errors = options.Validate();

        Assert.Single(errors);
        Assert.Contains("Host", errors[0]);
    }

    /// <summary>
    /// Проверяет, что пустой адрес отправителя даёт ошибку.
    /// </summary>
    [Fact]
    public void Validate_SmtpWithoutFrom_ReturnsError()
    {
        var options = CreateValidSmtpOptions();
        options.From = string.Empty;

        var errors = options.Validate();

        Assert.Single(errors);
        Assert.Contains("From", errors[0]);
    }

    /// <summary>
    /// Проверяет, что адрес отправителя неверного формата даёт ошибку.
    /// </summary>
    [Fact]
    public void Validate_SmtpWithInvalidFrom_ReturnsError()
    {
        var options = CreateValidSmtpOptions();
        options.From = "не адрес";

        Assert.Contains(options.Validate(), e => e.Contains("From"));
    }

    /// <summary>
    /// Проверяет, что порт вне диапазона 1–65535 даёт ошибку.
    /// </summary>
    /// <param name="port">Некорректный номер порта.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    public void Validate_SmtpPortOutOfRange_ReturnsError(int port)
    {
        var options = CreateValidSmtpOptions();
        options.Smtp.Port = port;

        Assert.Contains(options.Validate(), e => e.Contains("Порт"));
    }

    /// <summary>
    /// Проверяет, что неизвестный провайдер даёт ошибку.
    /// </summary>
    [Fact]
    public void Validate_UnknownProvider_ReturnsError()
    {
        var options = new EmailOptions { Provider = "Carrier pigeon" };

        var errors = options.Validate();

        Assert.Single(errors);
        Assert.Contains("Неизвестный провайдер", errors[0]);
    }

    /// <summary>
    /// Проверяет, что название провайдера принимается без учёта регистра.
    /// </summary>
    /// <param name="provider">Название провайдера в произвольном регистре.</param>
    [Theory]
    [InlineData("smtp")]
    [InlineData("SMTP")]
    [InlineData("Smtp")]
    public void Validate_ProviderIsCaseInsensitive(string provider)
    {
        var options = CreateValidSmtpOptions();
        options.Provider = provider;

        Assert.Empty(options.Validate());
    }
}
