using PotionCraft.Contracts.Identity;
using PotionCraft.Server.Services.Identity;
using PotionCraft.Tests.Infrastructure;

namespace PotionCraft.Tests.Services.Identity;

/// <summary>
/// Тесты для EmailConfirmationService: отправка, повторная отправка, проверка кода, попытки и срок действия.
/// Время управляется через <see cref="ManualTimeProvider"/>, письма перехватываются <see cref="FakeEmailSender"/>.
/// </summary>
public class EmailConfirmationServiceTests : IDisposable
{
    /// <summary>Адрес почты тестового пользователя.</summary>
    private const string Email = "hero@example.com";

    /// <summary>Тестовое окружение Identity.</summary>
    private readonly IdentityTestHost _host;

    /// <summary>Тестируемый сервис подтверждения почты.</summary>
    private readonly IEmailConfirmationService _service;

    /// <summary>
    /// Инициализирует тестовое окружение и сервис.
    /// </summary>
    public EmailConfirmationServiceTests()
    {
        _host = new IdentityTestHost();
        _service = _host.EmailConfirmation;
    }

    /// <summary>
    /// Освобождает тестовое окружение.
    /// </summary>
    public void Dispose()
    {
        _host.Dispose();
    }

    // ─── SendCodeAsync ────────────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, что отправка отправляет ровно одно письмо с шестизначным кодом на почту пользователя.
    /// </summary>
    [Fact]
    public async Task SendCodeAsync_NewUser_SendsOneEmailWithSixDigitCode()
    {
        var user = await CreateUnconfirmedUserAsync();

        var sent = await _service.SendCodeAsync(user);

        Assert.True(sent);
        var email = Assert.Single(_host.EmailSender.Sent);
        Assert.Equal(Email, email.To);
        Assert.Equal("Код подтверждения PotionCraft", email.Subject);
        Assert.Matches(@"^\d{6}$", _host.EmailSender.LastCode());
    }

    /// <summary>
    /// Проверяет, что в базе сохраняется только хэш кода, а срок действия равен текущему времени плюс 10 минут.
    /// </summary>
    [Fact]
    public async Task SendCodeAsync_StoresHashAndExpiry_NotPlainCode()
    {
        var user = await CreateUnconfirmedUserAsync();
        await _service.SendCodeAsync(user);
        var code = _host.EmailSender.LastCode();

        var stored = await StoredUserAsync();

        Assert.NotNull(stored.EmailConfirmationCodeHash);
        Assert.NotEqual(code, stored.EmailConfirmationCodeHash);
        Assert.Equal(64, stored.EmailConfirmationCodeHash.Length);
        Assert.Equal(_host.Clock.GetUtcNow() + EmailConfirmationService.CodeLifetime, stored.EmailConfirmationCodeExpiresAt);
        Assert.Equal(0, stored.EmailConfirmationAttempts);
    }

    /// <summary>
    /// Проверяет, что повторная отправка раньше чем через 60 секунд не отправляет письмо.
    /// </summary>
    [Fact]
    public async Task SendCodeAsync_WithinResendInterval_DoesNotSend()
    {
        var user = await CreateUnconfirmedUserAsync();
        await _service.SendCodeAsync(user);
        _host.Clock.Advance(TimeSpan.FromSeconds(59));

        var sent = await _service.SendCodeAsync(user);

        Assert.False(sent);
        Assert.Single(_host.EmailSender.Sent);
    }

    // ─── ResendCodeAsync ──────────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, что после 60 секунд повторная отправка присылает новый код, а старый код перестаёт работать.
    /// </summary>
    [Fact]
    public async Task ResendCodeAsync_AfterInterval_SendsNewCode_AndOldCodeStopsWorking()
    {
        var user = await CreateUnconfirmedUserAsync();
        await _service.SendCodeAsync(user);
        var oldCode = _host.EmailSender.LastCode();
        _host.Clock.Advance(TimeSpan.FromSeconds(60));

        await _service.ResendCodeAsync(Email);

        Assert.Equal(2, _host.EmailSender.Sent.Count);
        var newCode = _host.EmailSender.LastCode();
        var oldResult = await _service.ConfirmAsync(Email, oldCode);
        Assert.Equal(EmailConfirmationStatus.InvalidCode, oldResult.Status);
        var newResult = await _service.ConfirmAsync(Email, newCode);
        Assert.Equal(EmailConfirmationStatus.Success, newResult.Status);
    }

    /// <summary>
    /// Проверяет, что повторная отправка для несуществующего адреса ничего не отправляет.
    /// </summary>
    [Fact]
    public async Task ResendCodeAsync_UnknownEmail_DoesNotSend()
    {
        await _service.ResendCodeAsync("nobody@example.com");

        Assert.Empty(_host.EmailSender.Sent);
    }

    /// <summary>
    /// Проверяет, что повторная отправка для уже подтверждённой почты ничего не отправляет.
    /// </summary>
    [Fact]
    public async Task ResendCodeAsync_ConfirmedUser_DoesNotSend()
    {
        await _host.CreateUserAsync("hero", Email, "password123", UserRoles.Player, emailConfirmed: true);

        await _service.ResendCodeAsync(Email);

        Assert.Empty(_host.EmailSender.Sent);
    }

    // ─── ConfirmAsync ─────────────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, что верный код подтверждает почту и очищает данные кода.
    /// </summary>
    [Fact]
    public async Task ConfirmAsync_CorrectCode_ConfirmsEmailAndClearsCodeData()
    {
        var user = await CreateUnconfirmedUserAsync();
        await _service.SendCodeAsync(user);
        var code = _host.EmailSender.LastCode();

        var result = await _service.ConfirmAsync(Email, code);

        Assert.Equal(EmailConfirmationStatus.Success, result.Status);
        Assert.NotNull(result.User);
        var stored = await StoredUserAsync();
        Assert.True(stored.EmailConfirmed);
        Assert.Null(stored.EmailConfirmationCodeHash);
        Assert.Null(stored.EmailConfirmationCodeExpiresAt);
        Assert.Null(stored.EmailConfirmationCodeSentAt);
        Assert.Equal(0, stored.EmailConfirmationAttempts);
    }

    /// <summary>
    /// Проверяет, что код с пробелами по краям принимается после обрезки.
    /// </summary>
    [Fact]
    public async Task ConfirmAsync_CodeWithSpaces_IsTrimmed()
    {
        var user = await CreateUnconfirmedUserAsync();
        await _service.SendCodeAsync(user);
        var code = _host.EmailSender.LastCode();

        var result = await _service.ConfirmAsync(Email, $"  {code} ");

        Assert.Equal(EmailConfirmationStatus.Success, result.Status);
    }

    /// <summary>
    /// Проверяет, что неверный код возвращает InvalidCode и увеличивает счётчик попыток.
    /// </summary>
    [Fact]
    public async Task ConfirmAsync_WrongCode_ReturnsInvalidCodeAndIncrementsAttempts()
    {
        var user = await CreateUnconfirmedUserAsync();
        await _service.SendCodeAsync(user);
        var wrong = WrongCode(_host.EmailSender.LastCode());

        var result = await _service.ConfirmAsync(Email, wrong);

        Assert.Equal(EmailConfirmationStatus.InvalidCode, result.Status);
        Assert.Null(result.User);
        Assert.Equal(1, (await StoredUserAsync()).EmailConfirmationAttempts);
        Assert.False((await StoredUserAsync()).EmailConfirmed);
    }

    /// <summary>
    /// Проверяет, что после пяти неверных попыток даже верный код не принимается.
    /// </summary>
    [Fact]
    public async Task ConfirmAsync_FiveWrongAttempts_ThenCorrectCode_ReturnsTooManyAttempts()
    {
        var user = await CreateUnconfirmedUserAsync();
        await _service.SendCodeAsync(user);
        var code = _host.EmailSender.LastCode();
        var wrong = WrongCode(code);

        for (var i = 0; i < 4; i++)
            Assert.Equal(EmailConfirmationStatus.InvalidCode, (await _service.ConfirmAsync(Email, wrong)).Status);
        Assert.Equal(EmailConfirmationStatus.TooManyAttempts, (await _service.ConfirmAsync(Email, wrong)).Status);

        var result = await _service.ConfirmAsync(Email, code);

        Assert.Equal(EmailConfirmationStatus.TooManyAttempts, result.Status);
        Assert.False((await StoredUserAsync()).EmailConfirmed);
    }

    /// <summary>
    /// Проверяет, что код, срок действия которого истёк, возвращает Expired даже при верном значении.
    /// </summary>
    [Fact]
    public async Task ConfirmAsync_ExpiredCode_ReturnsExpired()
    {
        var user = await CreateUnconfirmedUserAsync();
        await _service.SendCodeAsync(user);
        var code = _host.EmailSender.LastCode();
        _host.Clock.Advance(EmailConfirmationService.CodeLifetime);

        var result = await _service.ConfirmAsync(Email, code);

        Assert.Equal(EmailConfirmationStatus.Expired, result.Status);
        Assert.False((await StoredUserAsync()).EmailConfirmed);
    }

    /// <summary>
    /// Проверяет, что для несуществующей почты возвращается InvalidCode без раскрытия существования пользователя.
    /// </summary>
    [Fact]
    public async Task ConfirmAsync_UnknownEmail_ReturnsInvalidCode()
    {
        var result = await _service.ConfirmAsync("nobody@example.com", "123456");

        Assert.Equal(EmailConfirmationStatus.InvalidCode, result.Status);
    }

    /// <summary>
    /// Проверяет, что при отсутствии активного кода возвращается InvalidCode.
    /// </summary>
    [Fact]
    public async Task ConfirmAsync_NoActiveCode_ReturnsInvalidCode()
    {
        await CreateUnconfirmedUserAsync();

        var result = await _service.ConfirmAsync(Email, "123456");

        Assert.Equal(EmailConfirmationStatus.InvalidCode, result.Status);
    }

    // ─── Вспомогательные методы ───────────────────────────────────────────────

    /// <summary>
    /// Создаёт пользователя с неподтверждённой почтой.
    /// </summary>
    /// <returns>Созданный пользователь.</returns>
    private Task<ApplicationUser> CreateUnconfirmedUserAsync()
        => _host.CreateUserAsync("hero", Email, "password123", UserRoles.Player, emailConfirmed: false);

    /// <summary>
    /// Загружает тестового пользователя из базы заново.
    /// </summary>
    /// <returns>Пользователь с актуальными данными.</returns>
    private async Task<ApplicationUser> StoredUserAsync()
        => await _host.UserManager.FindByEmailAsync(Email) ?? throw new InvalidOperationException("Пользователь не найден.");

    /// <summary>
    /// Возвращает код, отличающийся от заданного.
    /// </summary>
    /// <param name="code">Исходный код.</param>
    /// <returns>Неверный код того же формата.</returns>
    private static string WrongCode(string code)
        => code == "000000" ? "000001" : "000000";
}
