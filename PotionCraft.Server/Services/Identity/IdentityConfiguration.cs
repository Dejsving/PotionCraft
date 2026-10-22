using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace PotionCraft.Server.Services.Identity;

/// <summary>
/// Настройки ASP.NET Core Identity и cookie аутентификации приложения.
/// </summary>
public static class IdentityConfiguration
{
    /// <summary>Допустимые символы имени пользователя по умолчанию (латиница, цифры и символы).</summary>
    private const string DefaultUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    /// <summary>Дополнительные допустимые символы имени пользователя: кириллица.</summary>
    private const string CyrillicUserNameCharacters = "абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";

    /// <summary>Название cookie аутентификации.</summary>
    public const string CookieName = "PotionCraft.Auth";

    /// <summary>
    /// Настраивает политику паролей, блокировки и уникальности пользователей.
    /// </summary>
    /// <param name="options">Настройки Identity.</param>
    public static void ConfigureIdentityOptions(IdentityOptions options)
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredUniqueChars = 1;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters = DefaultUserNameCharacters + CyrillicUserNameCharacters;
    }

    /// <summary>
    /// Настраивает cookie аутентификации: HttpOnly, SameSite=Lax, срок жизни 14 дней и коды ответа вместо редиректов.
    /// </summary>
    /// <param name="options">Настройки cookie.</param>
    /// <param name="isDevelopment">Запущено ли приложение в режиме разработки (http без Secure).</param>
    public static void ConfigureCookie(CookieAuthenticationOptions options, bool isDevelopment)
    {
        options.Cookie.Name = CookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.None : CookieSecurePolicy.Always;

        // Срок жизни постоянного cookie (rememberMe=true) с продлением при активности.
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;

        // API не должен перенаправлять на страницы входа: возвращаем коды статуса.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    }
}
