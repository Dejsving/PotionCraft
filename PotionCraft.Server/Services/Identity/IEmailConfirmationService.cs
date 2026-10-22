using PotionCraft.Contracts.Identity;

namespace PotionCraft.Server.Services.Identity;

/// <summary>
/// Результат попытки подтверждения почты по коду.
/// </summary>
public enum EmailConfirmationStatus
{
    /// <summary>Код верный, почта подтверждена.</summary>
    Success,

    /// <summary>Код неверный, либо код или пользователь не найдены.</summary>
    InvalidCode,

    /// <summary>Срок действия кода истёк.</summary>
    Expired,

    /// <summary>Исчерпан лимит неверных попыток для текущего кода.</summary>
    TooManyAttempts
}

/// <summary>
/// Результат подтверждения почты.
/// </summary>
/// <param name="Status">Статус попытки.</param>
/// <param name="User">Подтверждённый пользователь (только при успехе).</param>
public record EmailConfirmationResult(EmailConfirmationStatus Status, ApplicationUser? User = null);

/// <summary>
/// Сервис подтверждения почты одноразовым кодом: генерация, отправка, проверка и повторная отправка.
/// </summary>
public interface IEmailConfirmationService
{
    /// <summary>
    /// Генерирует новый код, сохраняет его хэш и отправляет письмо пользователю.
    /// Повторная отправка возможна не чаще одного раза в 60 секунд.
    /// </summary>
    /// <param name="user">Пользователь, которому отправляется код.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns><c>true</c>, если код отправлен; <c>false</c>, если отправка ограничена частотой или не удалась.</returns>
    Task<bool> SendCodeAsync(ApplicationUser user, CancellationToken ct = default);

    /// <summary>
    /// Отправляет новый код по адресу почты, если пользователь существует, не подтверждён и лимит частоты соблюдён.
    /// Не сообщает, существует ли пользователь.
    /// </summary>
    /// <param name="email">Адрес электронной почты.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task ResendCodeAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Проверяет код подтверждения и при успехе отмечает почту подтверждённой.
    /// </summary>
    /// <param name="email">Адрес электронной почты.</param>
    /// <param name="code">Введённый код.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Результат подтверждения.</returns>
    Task<EmailConfirmationResult> ConfirmAsync(string email, string code, CancellationToken ct = default);
}
