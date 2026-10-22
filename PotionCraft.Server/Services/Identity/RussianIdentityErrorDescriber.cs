using Microsoft.AspNetCore.Identity;

namespace PotionCraft.Server.Services.Identity;

/// <summary>
/// Описания ошибок ASP.NET Core Identity на русском языке.
/// </summary>
public class RussianIdentityErrorDescriber : IdentityErrorDescriber
{
    /// <summary>Возвращает общую ошибку.</summary>
    public override IdentityError DefaultError()
        => Create(nameof(DefaultError), "Произошла неизвестная ошибка.");

    /// <summary>Возвращает ошибку конкурентного изменения данных.</summary>
    public override IdentityError ConcurrencyFailure()
        => Create(nameof(ConcurrencyFailure), "Данные были изменены другим пользователем. Обновите страницу и повторите попытку.");

    /// <summary>Возвращает ошибку неверного пароля.</summary>
    public override IdentityError PasswordMismatch()
        => Create(nameof(PasswordMismatch), "Неверный пароль.");

    /// <summary>Возвращает ошибку недействительного токена.</summary>
    public override IdentityError InvalidToken()
        => Create(nameof(InvalidToken), "Недействительный токен.");

    /// <summary>Возвращает ошибку при связывании уже привязанного логина.</summary>
    public override IdentityError LoginAlreadyAssociated()
        => Create(nameof(LoginAlreadyAssociated), "Этот логин уже связан с другим пользователем.");

    /// <summary>Возвращает ошибку некорректного имени пользователя.</summary>
    /// <param name="userName">Имя пользователя, которое не прошло проверку.</param>
    public override IdentityError InvalidUserName(string? userName)
        => Create(nameof(InvalidUserName),
            $"Имя пользователя «{userName}» некорректно. Допускаются буквы латиницы и кириллицы, цифры и символы - . _ @ +.");

    /// <summary>Возвращает ошибку занятого имени пользователя.</summary>
    /// <param name="userName">Занятое имя пользователя.</param>
    public override IdentityError DuplicateUserName(string userName)
        => Create(nameof(DuplicateUserName), $"Имя пользователя «{userName}» уже занято.");

    /// <summary>Возвращает ошибку некорректного адреса электронной почты.</summary>
    /// <param name="email">Адрес, который не прошёл проверку.</param>
    public override IdentityError InvalidEmail(string? email)
        => Create(nameof(InvalidEmail), $"Адрес электронной почты «{email}» некорректен.");

    /// <summary>Возвращает ошибку повторно использованной почты.</summary>
    /// <param name="email">Почта, которая уже зарегистрирована.</param>
    public override IdentityError DuplicateEmail(string email)
        => Create(nameof(DuplicateEmail), $"Пользователь с почтой «{email}» уже зарегистрирован.");

    /// <summary>Возвращает ошибку роли, которая уже назначена пользователю.</summary>
    /// <param name="role">Название роли.</param>
    public override IdentityError UserAlreadyInRole(string? role)
        => Create(nameof(UserAlreadyInRole), $"Пользователь уже имеет роль «{role}».");

    /// <summary>Возвращает ошибку отсутствия роли у пользователя.</summary>
    /// <param name="role">Название роли.</param>
    public override IdentityError UserNotInRole(string? role)
        => Create(nameof(UserNotInRole), $"Пользователь не имеет роли «{role}».");

    /// <summary>Возвращает ошибку повторного создания роли.</summary>
    /// <param name="role">Название роли.</param>
    public override IdentityError DuplicateRoleName(string role)
        => Create(nameof(DuplicateRoleName), $"Роль «{role}» уже существует.");

    /// <summary>Возвращает ошибку некорректного названия роли.</summary>
    /// <param name="role">Название роли.</param>
    public override IdentityError InvalidRoleName(string? role)
        => Create(nameof(InvalidRoleName), $"Название роли «{role}» некорректно.");

    /// <summary>Возвращает ошибку повторной установки пароля.</summary>
    public override IdentityError UserAlreadyHasPassword()
        => Create(nameof(UserAlreadyHasPassword), "У пользователя уже есть пароль.");

    /// <summary>Возвращает ошибку, если блокировка не включена.</summary>
    public override IdentityError UserLockoutNotEnabled()
        => Create(nameof(UserLockoutNotEnabled), "Блокировка не включена для этого пользователя.");

    /// <summary>Возвращает ошибку использования кода восстановления.</summary>
    public override IdentityError RecoveryCodeRedemptionFailed()
        => Create(nameof(RecoveryCodeRedemptionFailed), "Не удалось использовать код восстановления.");

    /// <summary>Возвращает ошибку слишком короткого пароля.</summary>
    /// <param name="length">Минимально допустимая длина пароля.</param>
    public override IdentityError PasswordTooShort(int length)
        => Create(nameof(PasswordTooShort), $"Пароль должен содержать не менее {length} символов.");

    /// <summary>Возвращает ошибку недостатка уникальных символов в пароле.</summary>
    /// <param name="uniqueChars">Минимальное количество уникальных символов.</param>
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
        => Create(nameof(PasswordRequiresUniqueChars), $"Пароль должен содержать не менее {uniqueChars} уникальных символов.");

    /// <summary>Возвращает ошибку отсутствия спецсимвола в пароле.</summary>
    public override IdentityError PasswordRequiresNonAlphanumeric()
        => Create(nameof(PasswordRequiresNonAlphanumeric), "Пароль должен содержать хотя бы один специальный символ.");

    /// <summary>Возвращает ошибку отсутствия цифры в пароле.</summary>
    public override IdentityError PasswordRequiresDigit()
        => Create(nameof(PasswordRequiresDigit), "Пароль должен содержать хотя бы одну цифру.");

    /// <summary>Возвращает ошибку отсутствия строчной буквы в пароле.</summary>
    public override IdentityError PasswordRequiresLower()
        => Create(nameof(PasswordRequiresLower), "Пароль должен содержать хотя бы одну строчную букву.");

    /// <summary>Возвращает ошибку отсутствия заглавной буквы в пароле.</summary>
    public override IdentityError PasswordRequiresUpper()
        => Create(nameof(PasswordRequiresUpper), "Пароль должен содержать хотя бы одну заглавную букву.");

    /// <summary>
    /// Создаёт объект ошибки Identity с указанным кодом и русским описанием.
    /// </summary>
    /// <param name="code">Код ошибки.</param>
    /// <param name="description">Описание ошибки на русском языке.</param>
    /// <returns>Объект <see cref="IdentityError"/>.</returns>
    private static IdentityError Create(string code, string description)
        => new() { Code = code, Description = description };
}
