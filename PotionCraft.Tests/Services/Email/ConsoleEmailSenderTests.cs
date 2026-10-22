using Microsoft.Extensions.Logging;
using PotionCraft.Server.Services.Email;
using PotionCraft.Tests.Infrastructure;

namespace PotionCraft.Tests.Services.Email;

/// <summary>
/// Тесты для ConsoleEmailSender: письмо с кодом записывается в лог уровня Information.
/// </summary>
public class ConsoleEmailSenderTests
{
    /// <summary>
    /// Проверяет, что письмо записывается в лог с пометкой заглушки, получателем, темой и кодом.
    /// </summary>
    [Fact]
    public async Task SendAsync_WritesEmailWithCodeToInformationLog()
    {
        var logger = new ListLogger<ConsoleEmailSender>();
        var sender = new ConsoleEmailSender(logger);

        await sender.SendAsync("hero@example.com", "Код подтверждения PotionCraft",
            "Ваш код подтверждения: 123456. Код действует 10 минут.");

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.Contains("[EMAIL-ЗАГЛУШКА]", entry.Message);
        Assert.Contains("Кому: hero@example.com", entry.Message);
        Assert.Contains("Тема: Код подтверждения PotionCraft", entry.Message);
        Assert.Contains("Ваш код подтверждения: 123456.", entry.Message);
    }
}
