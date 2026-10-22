using Microsoft.AspNetCore.Identity;

namespace PotionCraft.Contracts.Identity;

/// <summary>
/// Пользователь приложения с идентификатором типа <see cref="Guid"/>.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>Хэш действующего кода подтверждения почты (SHA-256 от идентификатора пользователя и кода) или <c>null</c>, если кода нет.</summary>
    public string? EmailConfirmationCodeHash { get; set; }

    /// <summary>Момент истечения срока действия кода подтверждения почты (UTC) или <c>null</c>, если кода нет.</summary>
    public DateTimeOffset? EmailConfirmationCodeExpiresAt { get; set; }

    /// <summary>Момент последней отправки кода подтверждения почты (UTC) или <c>null</c>, если код ещё не отправлялся.</summary>
    public DateTimeOffset? EmailConfirmationCodeSentAt { get; set; }

    /// <summary>Количество неверных попыток ввода текущего кода подтверждения почты.</summary>
    public int EmailConfirmationAttempts { get; set; }
}
