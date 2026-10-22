using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using PotionCraft.Contracts.Identity;
using PotionCraft.Server.Services.Email;

namespace PotionCraft.Server.Services.Identity;

/// <summary>
/// Подтверждение почты одноразовым 6-значным кодом. В базе хранится только хэш кода.
/// </summary>
public class EmailConfirmationService : IEmailConfirmationService
{
    /// <summary>Срок действия кода подтверждения.</summary>
    public static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);

    /// <summary>Максимальное количество неверных попыток для одного кода.</summary>
    public const int MaxAttempts = 5;

    /// <summary>Минимальный интервал между отправками кода одному пользователю.</summary>
    public static readonly TimeSpan ResendInterval = TimeSpan.FromSeconds(60);

    /// <summary>Тема письма с кодом подтверждения.</summary>
    private const string EmailSubject = "Код подтверждения PotionCraft";

    /// <summary>Менеджер пользователей.</summary>
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>Отправитель писем.</summary>
    private readonly IEmailSender _emailSender;

    /// <summary>Источник текущего времени (подменяется в тестах).</summary>
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Создаёт сервис подтверждения почты.
    /// </summary>
    /// <param name="userManager">Менеджер пользователей.</param>
    /// <param name="emailSender">Отправитель писем.</param>
    /// <param name="timeProvider">Источник текущего времени.</param>
    public EmailConfirmationService(UserManager<ApplicationUser> userManager, IEmailSender emailSender, TimeProvider timeProvider)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Генерирует новый код, сохраняет его хэш со сроком действия и отправляет письмо.
    /// Сохранение выполняется до отправки, чтобы в письме не оказался код, которого нет в базе.
    /// </summary>
    /// <param name="user">Пользователь, которому отправляется код.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns><c>true</c>, если код отправлен; иначе <c>false</c>.</returns>
    public async Task<bool> SendCodeAsync(ApplicationUser user, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(user.Email))
            return false;

        var now = _timeProvider.GetUtcNow();
        if (user.EmailConfirmationCodeSentAt is { } sentAt && now - sentAt < ResendInterval)
            return false;

        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        user.EmailConfirmationCodeHash = ComputeHash(user.Id, code);
        user.EmailConfirmationCodeExpiresAt = now + CodeLifetime;
        user.EmailConfirmationCodeSentAt = now;
        user.EmailConfirmationAttempts = 0;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return false;

        var body = $"Ваш код подтверждения: {code}. Код действует {(int)CodeLifetime.TotalMinutes} минут.";
        await _emailSender.SendAsync(user.Email, EmailSubject, body, ct);
        return true;
    }

    /// <summary>
    /// Отправляет новый код, если пользователь найден и ещё не подтверждён.
    /// </summary>
    /// <param name="email">Адрес электронной почты.</param>
    /// <param name="ct">Токен отмены операции.</param>
    public async Task ResendCodeAsync(string email, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user == null || user.EmailConfirmed)
            return;

        await SendCodeAsync(user, ct);
    }

    /// <summary>
    /// Проверяет код: сначала лимит попыток, затем срок действия, затем совпадение хэша.
    /// Неверный код увеличивает счётчик попыток. При успехе почта подтверждается, данные кода очищаются.
    /// </summary>
    /// <param name="email">Адрес электронной почты.</param>
    /// <param name="code">Введённый код.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Результат подтверждения.</returns>
    public async Task<EmailConfirmationResult> ConfirmAsync(string email, string code, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user == null || user.EmailConfirmed || user.EmailConfirmationCodeHash == null)
            return new EmailConfirmationResult(EmailConfirmationStatus.InvalidCode);

        if (user.EmailConfirmationAttempts >= MaxAttempts)
            return new EmailConfirmationResult(EmailConfirmationStatus.TooManyAttempts);

        var now = _timeProvider.GetUtcNow();
        if (user.EmailConfirmationCodeExpiresAt is not { } expiresAt || now >= expiresAt)
            return new EmailConfirmationResult(EmailConfirmationStatus.Expired);

        var expectedHash = Encoding.UTF8.GetBytes(user.EmailConfirmationCodeHash);
        var actualHash = Encoding.UTF8.GetBytes(ComputeHash(user.Id, code.Trim()));
        if (!CryptographicOperations.FixedTimeEquals(expectedHash, actualHash))
        {
            user.EmailConfirmationAttempts++;
            var failedUpdate = await _userManager.UpdateAsync(user);
            if (!failedUpdate.Succeeded)
                return new EmailConfirmationResult(EmailConfirmationStatus.InvalidCode);

            var status = user.EmailConfirmationAttempts >= MaxAttempts
                ? EmailConfirmationStatus.TooManyAttempts
                : EmailConfirmationStatus.InvalidCode;
            return new EmailConfirmationResult(status);
        }

        user.EmailConfirmed = true;
        user.EmailConfirmationCodeHash = null;
        user.EmailConfirmationCodeExpiresAt = null;
        user.EmailConfirmationCodeSentAt = null;
        user.EmailConfirmationAttempts = 0;

        var updateResult = await _userManager.UpdateAsync(user);
        return updateResult.Succeeded
            ? new EmailConfirmationResult(EmailConfirmationStatus.Success, user)
            : new EmailConfirmationResult(EmailConfirmationStatus.InvalidCode);
    }

    /// <summary>
    /// Вычисляет SHA-256 от идентификатора пользователя и кода в шестнадцатеричном виде.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="code">Код подтверждения.</param>
    /// <returns>Хэш в виде строки из шестнадцатеричных символов.</returns>
    private static string ComputeHash(Guid userId, string code)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{userId}:{code}")));
}
