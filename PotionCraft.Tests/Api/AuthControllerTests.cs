using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PotionCraft.Contracts.Identity;
using PotionCraft.Server.Controllers;
using PotionCraft.Server.Services.Identity;
using PotionCraft.Tests.Infrastructure;

namespace PotionCraft.Tests.Api;

/// <summary>
/// Тесты для AuthController (регистрация, подтверждение почты, вход, выход, текущий пользователь).
/// Используют реальные Identity-менеджеры поверх SQLite in-memory.
/// </summary>
public class AuthControllerTests : IDisposable
{
    /// <summary>Адрес почты тестового пользователя.</summary>
    private const string Email = "hero@example.com";

    /// <summary>Тестовое окружение Identity.</summary>
    private readonly IdentityTestHost _host;

    /// <summary>Тестируемый контроллер.</summary>
    private readonly AuthController _controller;

    /// <summary>
    /// Инициализирует тестовое окружение и контроллер.
    /// </summary>
    public AuthControllerTests()
    {
        _host = new IdentityTestHost();
        _controller = new AuthController(_host.UserManager, _host.SignInManager, _host.EmailConfirmation)
        {
            ControllerContext = _host.CreateControllerContext()
        };
    }

    /// <summary>
    /// Освобождает тестовое окружение.
    /// </summary>
    public void Dispose()
    {
        _host.Dispose();
    }

    // ─── Register ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, что регистрация создаёт неподтверждённого пользователя с ролью Player и возвращает признак подтверждения.
    /// </summary>
    [Fact]
    public async Task Register_ValidData_ReturnsOkWithConfirmationRequired()
    {
        var result = await _controller.Register(new RegisterRequest("hero", Email, "password123"));

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<RegisterResponse>(ok.Value);
        Assert.Equal(Email, response.Email);
        Assert.True(response.RequiresEmailConfirmation);

        var stored = await _host.UserManager.FindByNameAsync("hero");
        Assert.NotNull(stored);
        Assert.False(stored.EmailConfirmed);
        Assert.Equal(new[] { UserRoles.Player }, await _host.UserManager.GetRolesAsync(stored));
    }

    /// <summary>
    /// Проверяет, что регистрация отправляет ровно одно письмо с кодом и не выполняет вход.
    /// </summary>
    [Fact]
    public async Task Register_SendsOneEmailWithCode_AndDoesNotSignIn()
    {
        await _controller.Register(new RegisterRequest("hero", Email, "password123"));

        var email = Assert.Single(_host.EmailSender.Sent);
        Assert.Equal(Email, email.To);
        Assert.Matches(@"^\d{6}$", _host.EmailSender.LastCode());
        Assert.False(_controller.HttpContext.Response.Headers.ContainsKey("Set-Cookie"));
    }

    /// <summary>
    /// Проверяет, что имя пользователя из кириллицы допустимо при регистрации.
    /// </summary>
    [Fact]
    public async Task Register_CyrillicUserName_ReturnsOk()
    {
        var result = await _controller.Register(new RegisterRequest("Ёлка", "yolka@example.com", "password123"));

        Assert.IsType<OkObjectResult>(result);
    }

    /// <summary>
    /// Проверяет, что повторное имя пользователя возвращает 400 с сообщением на русском языке.
    /// </summary>
    [Fact]
    public async Task Register_DuplicateUserName_Returns400WithRussianMessage()
    {
        await _host.CreateUserAsync("hero", "first@example.com", "password123", UserRoles.Player);

        var result = await _controller.Register(new RegisterRequest("hero", "second@example.com", "password123"));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ValidationErrorResponse>(badRequest.Value);
        Assert.Contains("Имя пользователя «hero» уже занято.", response.Errors);
        Assert.Equal(string.Join(" ", response.Errors), response.Message);
    }

    /// <summary>
    /// Проверяет, что повторный адрес электронной почты возвращает 400.
    /// </summary>
    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        await _host.CreateUserAsync("first", Email, "password123", UserRoles.Player);

        var result = await _controller.Register(new RegisterRequest("second", Email, "password123"));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ValidationErrorResponse>(badRequest.Value);
        Assert.Contains(response.Errors, e => e.Contains("уже зарегистрирован"));
    }

    /// <summary>
    /// Проверяет, что короткий пароль возвращает 400 с сообщением о минимальной длине.
    /// </summary>
    [Fact]
    public async Task Register_ShortPassword_Returns400WithLengthMessage()
    {
        var result = await _controller.Register(new RegisterRequest("hero", Email, "short"));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ValidationErrorResponse>(badRequest.Value);
        Assert.Contains("Пароль должен содержать не менее 8 символов.", response.Errors);
    }

    /// <summary>
    /// Проверяет, что пустые поля возвращают 400 со списком ошибок валидации.
    /// </summary>
    [Fact]
    public async Task Register_EmptyFields_Returns400WithErrors()
    {
        var result = await _controller.Register(new RegisterRequest(" ", "", ""));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ValidationErrorResponse>(badRequest.Value);
        Assert.Equal(3, response.Errors.Count);
    }

    // ─── ConfirmEmail ─────────────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, что верный код подтверждает почту и выполняет вход, возвращая текущего пользователя.
    /// </summary>
    [Fact]
    public async Task ConfirmEmail_CorrectCode_ReturnsOkAndSignsIn()
    {
        await _controller.Register(new RegisterRequest("hero", Email, "password123"));
        var code = _host.EmailSender.LastCode();

        var result = await _controller.ConfirmEmail(new ConfirmEmailRequest(Email, code));

        var ok = Assert.IsType<OkObjectResult>(result);
        var summary = Assert.IsType<UserSummary>(ok.Value);
        Assert.Equal("hero", summary.UserName);
        Assert.Equal(new[] { UserRoles.Player }, summary.Roles);
        Assert.True((await _host.UserManager.FindByNameAsync("hero"))!.EmailConfirmed);
        Assert.True(_controller.HttpContext.Response.Headers.ContainsKey("Set-Cookie"));
    }

    /// <summary>
    /// Проверяет, что неверный код возвращает 400 с сообщением «Неверный код.».
    /// </summary>
    [Fact]
    public async Task ConfirmEmail_WrongCode_Returns400WithInvalidCodeMessage()
    {
        await _controller.Register(new RegisterRequest("hero", Email, "password123"));

        var result = await _controller.ConfirmEmail(new ConfirmEmailRequest(Email, "000000"));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Неверный код.", GetMessage(badRequest.Value));
        Assert.False(_controller.HttpContext.Response.Headers.ContainsKey("Set-Cookie"));
    }

    /// <summary>
    /// Проверяет, что после пяти неверных попыток возвращается сообщение об исчерпании попыток даже для верного кода.
    /// </summary>
    [Fact]
    public async Task ConfirmEmail_FiveWrongAttempts_ReturnsTooManyAttemptsMessageEvenWithCorrectCode()
    {
        await _controller.Register(new RegisterRequest("hero", Email, "password123"));
        var code = _host.EmailSender.LastCode();
        var wrong = code == "000000" ? "000001" : "000000";

        for (var i = 0; i < 5; i++)
            await _controller.ConfirmEmail(new ConfirmEmailRequest(Email, wrong));
        var result = await _controller.ConfirmEmail(new ConfirmEmailRequest(Email, code));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Слишком много неверных попыток. Запросите новый код.", GetMessage(badRequest.Value));
    }

    /// <summary>
    /// Проверяет, что код с истёкшим сроком возвращает сообщение об истечении срока.
    /// </summary>
    [Fact]
    public async Task ConfirmEmail_ExpiredCode_ReturnsExpiredMessage()
    {
        await _controller.Register(new RegisterRequest("hero", Email, "password123"));
        var code = _host.EmailSender.LastCode();
        _host.Clock.Advance(EmailConfirmationService.CodeLifetime);

        var result = await _controller.ConfirmEmail(new ConfirmEmailRequest(Email, code));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Срок действия кода истёк. Запросите новый код.", GetMessage(badRequest.Value));
    }

    /// <summary>
    /// Проверяет, что для неизвестной почты возвращается тот же ответ «Неверный код.».
    /// </summary>
    [Fact]
    public async Task ConfirmEmail_UnknownEmail_Returns400WithInvalidCodeMessage()
    {
        var result = await _controller.ConfirmEmail(new ConfirmEmailRequest("nobody@example.com", "123456"));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Неверный код.", GetMessage(badRequest.Value));
    }

    // ─── ResendCode ───────────────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, что повторная отправка возвращает 204 и отправляет письмо только после 60 секунд.
    /// </summary>
    [Fact]
    public async Task ResendCode_SendsOnlyAfterSixtySeconds_AndReturnsNoContent()
    {
        await _controller.Register(new RegisterRequest("hero", Email, "password123"));

        var early = await _controller.ResendCode(new ResendCodeRequest(Email));
        Assert.IsType<NoContentResult>(early);
        Assert.Single(_host.EmailSender.Sent);

        _host.Clock.Advance(TimeSpan.FromSeconds(60));
        var late = await _controller.ResendCode(new ResendCodeRequest(Email));

        Assert.IsType<NoContentResult>(late);
        Assert.Equal(2, _host.EmailSender.Sent.Count);
    }

    /// <summary>
    /// Проверяет, что для неизвестной почты повторная отправка возвращает 204 без отправки письма.
    /// </summary>
    [Fact]
    public async Task ResendCode_UnknownEmail_ReturnsNoContentWithoutEmail()
    {
        var result = await _controller.ResendCode(new ResendCodeRequest("nobody@example.com"));

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(_host.EmailSender.Sent);
    }

    // ─── Login ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, что вход по имени пользователя с верным паролем возвращает 200.
    /// </summary>
    [Fact]
    public async Task Login_ByUserName_ReturnsOkWithCurrentUser()
    {
        await _host.CreateUserAsync("hero", Email, "password123", UserRoles.Player);

        var result = await _controller.Login(new LoginRequest("hero", "password123", false));

        var ok = Assert.IsType<OkObjectResult>(result);
        var summary = Assert.IsType<UserSummary>(ok.Value);
        Assert.Equal("hero", summary.UserName);
    }

    /// <summary>
    /// Проверяет, что вход по адресу электронной почты работает.
    /// </summary>
    [Fact]
    public async Task Login_ByEmail_ReturnsOk()
    {
        await _host.CreateUserAsync("hero", Email, "password123", UserRoles.Player);

        var result = await _controller.Login(new LoginRequest(Email, "password123", true));

        var ok = Assert.IsType<OkObjectResult>(result);
        var summary = Assert.IsType<UserSummary>(ok.Value);
        Assert.Equal("hero", summary.UserName);
    }

    /// <summary>
    /// Проверяет, что неверный пароль возвращает 401 с сообщением о неверном логине или пароле.
    /// </summary>
    [Fact]
    public async Task Login_WrongPassword_Returns401WithMessage()
    {
        await _host.CreateUserAsync("hero", Email, "password123", UserRoles.Player);

        var result = await _controller.Login(new LoginRequest("hero", "wrongpass", false));

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Неверный логин или пароль.", GetMessage(unauthorized.Value));
    }

    /// <summary>
    /// Проверяет, что несуществующий пользователь возвращает 401 с тем же сообщением.
    /// </summary>
    [Fact]
    public async Task Login_UnknownUser_Returns401WithSameMessage()
    {
        var result = await _controller.Login(new LoginRequest("nobody", "password123", false));

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Неверный логин или пароль.", GetMessage(unauthorized.Value));
    }

    /// <summary>
    /// Проверяет, что после пяти неудачных попыток вход блокируется с сообщением о блокировке.
    /// </summary>
    [Fact]
    public async Task Login_AfterFiveFailedAttempts_ReturnsLockedOutMessage()
    {
        await _host.CreateUserAsync("hero", Email, "password123", UserRoles.Player);

        for (var i = 0; i < 5; i++)
            await _controller.Login(new LoginRequest("hero", "wrongpass", false));

        var result = await _controller.Login(new LoginRequest("hero", "password123", false));

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Слишком много неудачных попыток входа. Попробуйте снова через 5 минут.", GetMessage(unauthorized.Value));
    }

    /// <summary>
    /// Проверяет, что неподтверждённый пользователь с верным паролем получает 403, код отправляется, а вход не выполняется.
    /// </summary>
    [Fact]
    public async Task Login_UnconfirmedUserWithCorrectPassword_Returns403_SendsCode_AndDoesNotSignIn()
    {
        await _host.CreateUserAsync("hero", Email, "password123", UserRoles.Player, emailConfirmed: false);

        var result = await _controller.Login(new LoginRequest("hero", "password123", false));

        var forbidden = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        Assert.Equal($"Email не подтверждён. Мы отправили код подтверждения на {Email}.", GetMessage(forbidden.Value));
        Assert.Equal(true, GetProperty(forbidden.Value, "requiresEmailConfirmation"));
        Assert.Equal(Email, GetProperty(forbidden.Value, "email"));
        Assert.Single(_host.EmailSender.Sent);
        Assert.False(_controller.HttpContext.Response.Headers.ContainsKey("Set-Cookie"));
    }

    /// <summary>
    /// Проверяет, что повторный вход неподтверждённого пользователя в течение 60 секунд не отправляет новый код.
    /// </summary>
    [Fact]
    public async Task Login_UnconfirmedUserTwiceWithinInterval_SendsCodeOnlyOnce()
    {
        await _host.CreateUserAsync("hero", Email, "password123", UserRoles.Player, emailConfirmed: false);

        await _controller.Login(new LoginRequest("hero", "password123", false));
        var second = await _controller.Login(new LoginRequest("hero", "password123", false));

        Assert.IsType<ObjectResult>(second);
        Assert.Equal(StatusCodes.Status403Forbidden, ((ObjectResult)second).StatusCode);
        Assert.Single(_host.EmailSender.Sent);
    }

    /// <summary>
    /// Проверяет, что неподтверждённый пользователь с неверным паролем получает 401 и письма не отправляются.
    /// </summary>
    [Fact]
    public async Task Login_UnconfirmedUserWithWrongPassword_Returns401_AndSendsNothing()
    {
        await _host.CreateUserAsync("hero", Email, "password123", UserRoles.Player, emailConfirmed: false);

        var result = await _controller.Login(new LoginRequest("hero", "wrongpass", false));

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Неверный логин или пароль.", GetMessage(unauthorized.Value));
        Assert.Empty(_host.EmailSender.Sent);
    }

    // ─── Logout / Me ──────────────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, что выход возвращает 204 No Content.
    /// </summary>
    [Fact]
    public async Task Logout_ReturnsNoContent()
    {
        var result = await _controller.Logout();

        Assert.IsType<NoContentResult>(result);
    }

    /// <summary>
    /// Проверяет, что Me возвращает данные текущего пользователя.
    /// </summary>
    [Fact]
    public async Task Me_AuthenticatedUser_ReturnsCurrentUser()
    {
        var user = await _host.CreateUserAsync("hero", Email, "password123", UserRoles.Player);
        _controller.ControllerContext = _host.CreateControllerContext(IdentityTestHost.CreatePrincipal(user.Id, UserRoles.Player));

        var result = await _controller.Me();

        var ok = Assert.IsType<OkObjectResult>(result);
        var summary = Assert.IsType<UserSummary>(ok.Value);
        Assert.Equal(user.Id, summary.Id);
    }

    /// <summary>
    /// Проверяет, что Me возвращает 401, если пользователь не аутентифицирован.
    /// </summary>
    [Fact]
    public async Task Me_Anonymous_Returns401()
    {
        var result = await _controller.Me();

        Assert.IsType<UnauthorizedResult>(result);
    }

    /// <summary>
    /// Извлекает свойство message из анонимного объекта ответа.
    /// </summary>
    /// <param name="value">Объект ответа.</param>
    /// <returns>Текст сообщения.</returns>
    private static string? GetMessage(object? value)
    {
        return GetProperty(value, "message")?.ToString();
    }

    /// <summary>
    /// Извлекает свойство из анонимного объекта ответа по имени.
    /// </summary>
    /// <param name="value">Объект ответа.</param>
    /// <param name="name">Имя свойства.</param>
    /// <returns>Значение свойства или <c>null</c>.</returns>
    private static object? GetProperty(object? value, string name)
    {
        return value?.GetType().GetProperty(name)?.GetValue(value);
    }
}
