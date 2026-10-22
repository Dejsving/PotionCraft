using PotionCraft.Server.Services.Identity;

namespace PotionCraft.Tests.Services.Identity;

/// <summary>
/// Тесты для RussianIdentityErrorDescriber: описания ошибок Identity на русском языке.
/// </summary>
public class RussianIdentityErrorDescriberTests
{
    /// <summary>Тестируемый описатель ошибок.</summary>
    private readonly RussianIdentityErrorDescriber _describer = new();

    /// <summary>
    /// Проверяет, что ошибка занятого имени пользователя содержит имя и русский текст.
    /// </summary>
    [Fact]
    public void DuplicateUserName_ReturnsRussianDescriptionWithName()
    {
        var error = _describer.DuplicateUserName("hero");

        Assert.Equal(nameof(_describer.DuplicateUserName), error.Code);
        Assert.Equal("Имя пользователя «hero» уже занято.", error.Description);
    }

    /// <summary>
    /// Проверяет, что ошибка повторной почты содержит адрес и русский текст.
    /// </summary>
    [Fact]
    public void DuplicateEmail_ReturnsRussianDescriptionWithEmail()
    {
        var error = _describer.DuplicateEmail("hero@example.com");

        Assert.Equal("Пользователь с почтой «hero@example.com» уже зарегистрирован.", error.Description);
    }

    /// <summary>
    /// Проверяет, что ошибка короткого пароля содержит минимальную длину.
    /// </summary>
    [Fact]
    public void PasswordTooShort_ReturnsRussianDescriptionWithLength()
    {
        var error = _describer.PasswordTooShort(8);

        Assert.Equal("Пароль должен содержать не менее 8 символов.", error.Description);
    }

    /// <summary>
    /// Проверяет, что общая ошибка возвращается на русском языке.
    /// </summary>
    [Fact]
    public void DefaultError_ReturnsRussianDescription()
    {
        var error = _describer.DefaultError();

        Assert.Equal("Произошла неизвестная ошибка.", error.Description);
    }
}
