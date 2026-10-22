using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PotionCraft.Server.Services.Email;

namespace PotionCraft.Tests.Services.Email;

/// <summary>
/// Тесты для AddEmailSender: выбор реализации IEmailSender по настройкам.
/// </summary>
public class EmailServiceCollectionExtensionsTests
{
    /// <summary>
    /// Собирает провайдер сервисов с регистрацией отправителя почты по переданным настройкам.
    /// </summary>
    /// <param name="settings">Ключи и значения секции "Email".</param>
    /// <returns>Построенный провайдер сервисов.</returns>
    private static ServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEmailSender(configuration);
        return services.BuildServiceProvider();
    }

    /// <summary>Корректные SMTP-настройки для тестов.</summary>
    private static readonly Dictionary<string, string?> ValidSmtpSettings = new()
    {
        ["Email:Provider"] = "Smtp",
        ["Email:From"] = "PotionCraft <noreply@example.com>",
        ["Email:Smtp:Host"] = "smtp.example.com",
        ["Email:Smtp:Port"] = "587"
    };

    /// <summary>
    /// Проверяет, что без настроек провайдера по умолчанию регистрируется ConsoleEmailSender.
    /// </summary>
    [Fact]
    public void AddEmailSender_WithoutProvider_RegistersConsoleEmailSender()
    {
        using var provider = BuildProvider(new Dictionary<string, string?>());

        Assert.IsType<ConsoleEmailSender>(provider.GetRequiredService<IEmailSender>());
    }

    /// <summary>
    /// Проверяет, что явный провайдер Console регистрирует ConsoleEmailSender.
    /// </summary>
    [Fact]
    public void AddEmailSender_ConsoleProvider_RegistersConsoleEmailSender()
    {
        using var provider = BuildProvider(new Dictionary<string, string?> { ["Email:Provider"] = "Console" });

        Assert.IsType<ConsoleEmailSender>(provider.GetRequiredService<IEmailSender>());
    }

    /// <summary>
    /// Проверяет, что провайдер Smtp с корректными настройками регистрирует SmtpEmailSender.
    /// </summary>
    [Fact]
    public void AddEmailSender_SmtpProvider_RegistersSmtpEmailSender()
    {
        using var provider = BuildProvider(ValidSmtpSettings);

        Assert.IsType<SmtpEmailSender>(provider.GetRequiredService<IEmailSender>());
    }

    /// <summary>
    /// Проверяет, что провайдер Smtp в любом регистре регистрирует SmtpEmailSender.
    /// </summary>
    [Fact]
    public void AddEmailSender_SmtpProviderIsCaseInsensitive()
    {
        var settings = new Dictionary<string, string?>(ValidSmtpSettings)
        {
            ["Email:Provider"] = "SMTP"
        };

        using var provider = BuildProvider(settings);

        Assert.IsType<SmtpEmailSender>(provider.GetRequiredService<IEmailSender>());
    }

    /// <summary>
    /// Проверяет, что провайдер Smtp без адреса сервера приводит к исключению при регистрации.
    /// </summary>
    [Fact]
    public void AddEmailSender_SmtpWithoutHost_Throws()
    {
        var settings = new Dictionary<string, string?>(ValidSmtpSettings);
        settings.Remove("Email:Smtp:Host");

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var exception = Assert.Throws<InvalidOperationException>(() => services.AddEmailSender(configuration));
        Assert.Contains("Host", exception.Message);
    }

    /// <summary>
    /// Проверяет, что неизвестный провайдер приводит к исключению при регистрации.
    /// </summary>
    [Fact]
    public void AddEmailSender_UnknownProvider_Throws()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Email:Provider"] = "Pigeon" })
            .Build();

        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddEmailSender(configuration));
    }
}
