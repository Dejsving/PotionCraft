using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PotionCraft.Server.Services.Email;

/// <summary>
/// Методы регистрации отправителя электронной почты в контейнере зависимостей.
/// </summary>
public static class EmailServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует IEmailSender по настройкам секции "Email".
    /// При провайдере "Smtp" используется SmtpEmailSender, иначе ConsoleEmailSender.
    /// </summary>
    /// <param name="services">Коллекция сервисов приложения.</param>
    /// <param name="configuration">Конфигурация, из которой читаются настройки почты.</param>
    /// <returns>Та же коллекция сервисов для цепочки вызовов.</returns>
    /// <exception cref="InvalidOperationException">Настройки почты некорректны.</exception>
    public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(EmailOptions.SectionName);
        services.Configure<EmailOptions>(section);

        var options = section.Get<EmailOptions>() ?? new EmailOptions();
        var errors = options.Validate();
        if (errors.Count > 0)
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors));

        if (string.Equals(options.Provider, EmailOptions.SmtpProvider, StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
        else
            services.AddSingleton<IEmailSender, ConsoleEmailSender>();

        return services;
    }
}
