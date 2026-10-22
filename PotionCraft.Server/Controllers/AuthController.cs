using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PotionCraft.Contracts.Identity;
using PotionCraft.Server.Services.Identity;

namespace PotionCraft.Server.Controllers
{
    /// <summary>
    /// Запрос на регистрацию нового пользователя.
    /// </summary>
    /// <param name="UserName">Имя пользователя.</param>
    /// <param name="Email">Адрес электронной почты.</param>
    /// <param name="Password">Пароль.</param>
    public record RegisterRequest(string UserName, string Email, string Password);

    /// <summary>
    /// Ответ на регистрацию: почта, на которую отправлен код, и признак необходимости подтверждения.
    /// </summary>
    /// <param name="Email">Адрес электронной почты, на который отправлен код.</param>
    /// <param name="RequiresEmailConfirmation">Требуется ли подтверждение почты перед входом.</param>
    public record RegisterResponse(string Email, bool RequiresEmailConfirmation);

    /// <summary>
    /// Запрос на вход в систему.
    /// </summary>
    /// <param name="Login">Имя пользователя или адрес электронной почты.</param>
    /// <param name="Password">Пароль.</param>
    /// <param name="RememberMe">Запомнить вход (постоянный cookie на 14 дней).</param>
    public record LoginRequest(string Login, string Password, bool RememberMe);

    /// <summary>
    /// Запрос на подтверждение почты одноразовым кодом.
    /// </summary>
    /// <param name="Email">Адрес электронной почты.</param>
    /// <param name="Code">Код подтверждения из письма.</param>
    /// <param name="RememberMe">Запомнить вход после подтверждения (постоянный cookie на 14 дней).</param>
    public record ConfirmEmailRequest(string Email, string Code, bool RememberMe = false);

    /// <summary>
    /// Запрос на повторную отправку кода подтверждения.
    /// </summary>
    /// <param name="Email">Адрес электронной почты.</param>
    public record ResendCodeRequest(string Email);

    /// <summary>
    /// Ответ с описанием ошибок валидации.
    /// </summary>
    /// <param name="Message">Общее сообщение об ошибке.</param>
    /// <param name="Errors">Список ошибок.</param>
    public record ValidationErrorResponse(string Message, IReadOnlyList<string> Errors);

    /// <summary>
    /// Регистрация, подтверждение почты, вход, выход и получение текущего пользователя.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        /// <summary>Сообщение при неверном логине или пароле.</summary>
        private const string InvalidCredentialsMessage = "Неверный логин или пароль.";

        /// <summary>Сообщение при блокировке учётной записи после неудачных попыток входа.</summary>
        private const string LockedOutMessage = "Слишком много неудачных попыток входа. Попробуйте снова через 5 минут.";

        /// <summary>Сообщение при неверном или не найденном коде подтверждения.</summary>
        private const string InvalidCodeMessage = "Неверный код.";

        /// <summary>Сообщение при истекшем коде подтверждения.</summary>
        private const string ExpiredCodeMessage = "Срок действия кода истёк. Запросите новый код.";

        /// <summary>Сообщение при исчерпании попыток ввода кода.</summary>
        private const string TooManyAttemptsMessage = "Слишком много неверных попыток. Запросите новый код.";

        /// <summary>Менеджер пользователей.</summary>
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>Менеджер входа в систему.</summary>
        private readonly SignInManager<ApplicationUser> _signInManager;

        /// <summary>Сервис подтверждения почты одноразовым кодом.</summary>
        private readonly IEmailConfirmationService _emailConfirmationService;

        /// <summary>
        /// Создаёт контроллер аутентификации.
        /// </summary>
        /// <param name="userManager">Менеджер пользователей.</param>
        /// <param name="signInManager">Менеджер входа в систему.</param>
        /// <param name="emailConfirmationService">Сервис подтверждения почты.</param>
        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IEmailConfirmationService emailConfirmationService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailConfirmationService = emailConfirmationService;
        }

        /// <summary>
        /// Регистрирует нового пользователя с ролью Player, отправляет код подтверждения почты.
        /// Вход не выполняется: пользователь сможет войти только после подтверждения почты.
        /// </summary>
        /// <param name="request">Данные регистрации.</param>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(request.UserName))
                errors.Add("Необходимо ввести имя пользователя.");
            if (string.IsNullOrWhiteSpace(request.Email))
                errors.Add("Необходимо ввести адрес электронной почты.");
            if (string.IsNullOrWhiteSpace(request.Password))
                errors.Add("Необходимо ввести пароль.");

            if (errors.Count > 0)
                return BadRequest(new ValidationErrorResponse(string.Join(" ", errors), errors));

            var user = new ApplicationUser
            {
                UserName = request.UserName.Trim(),
                Email = request.Email.Trim()
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
                return BadRequest(ToValidationError(createResult));

            var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.Player);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return BadRequest(ToValidationError(roleResult));
            }

            await _emailConfirmationService.SendCodeAsync(user);

            return Ok(new RegisterResponse(user.Email ?? string.Empty, RequiresEmailConfirmation: true));
        }

        /// <summary>
        /// Выполняет вход по имени пользователя или адресу электронной почты.
        /// Если пароль верный, но почта не подтверждена, отправляет код и возвращает 403.
        /// </summary>
        /// <param name="request">Логин, пароль и признак запоминания входа.</param>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Необходимо ввести логин и пароль." });

            var user = await _userManager.FindByNameAsync(request.Login)
                ?? await _userManager.FindByEmailAsync(request.Login);
            if (user == null)
                return Unauthorized(new { message = InvalidCredentialsMessage });

            // Проверка пароля без RequireConfirmedEmail: иначе PreSignInCheck вернёт NotAllowed до проверки пароля,
            // и неподтверждённый пользователь не сможет отличить верный пароль от неверного.
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (result.IsLockedOut)
                return Unauthorized(new { message = LockedOutMessage });

            if (!result.Succeeded)
                return Unauthorized(new { message = InvalidCredentialsMessage });

            if (!user.EmailConfirmed)
            {
                await _emailConfirmationService.SendCodeAsync(user);
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = $"Email не подтверждён. Мы отправили код подтверждения на {user.Email}.",
                    requiresEmailConfirmation = true,
                    email = user.Email
                });
            }

            await _signInManager.SignInAsync(user, request.RememberMe);

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(UserSummary.FromUser(user, roles));
        }

        /// <summary>
        /// Подтверждает почту по коду, отмечает её подтверждённой и выполняет вход.
        /// </summary>
        /// <param name="request">Адрес почты, код и признак запоминания входа.</param>
        [HttpPost("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest(new { message = InvalidCodeMessage });

            var result = await _emailConfirmationService.ConfirmAsync(request.Email, request.Code);
            switch (result.Status)
            {
                case EmailConfirmationStatus.Success:
                    var user = result.User!;
                    await _signInManager.SignInAsync(user, request.RememberMe);
                    var roles = await _userManager.GetRolesAsync(user);
                    return Ok(UserSummary.FromUser(user, roles));
                case EmailConfirmationStatus.Expired:
                    return BadRequest(new { message = ExpiredCodeMessage });
                case EmailConfirmationStatus.TooManyAttempts:
                    return BadRequest(new { message = TooManyAttemptsMessage });
                default:
                    return BadRequest(new { message = InvalidCodeMessage });
            }
        }

        /// <summary>
        /// Повторно отправляет код подтверждения. Всегда возвращает 204, чтобы не раскрывать наличие пользователя.
        /// </summary>
        /// <param name="request">Адрес электронной почты.</param>
        [HttpPost("resend-code")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendCode([FromBody] ResendCodeRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Email))
                await _emailConfirmationService.ResendCodeAsync(request.Email);

            return NoContent();
        }

        /// <summary>
        /// Выход из системы: удаляет cookie аутентификации.
        /// </summary>
        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return NoContent();
        }

        /// <summary>
        /// Возвращает текущего вошедшего пользователя или 401, если пользователь не аутентифицирован.
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(UserSummary.FromUser(user, roles));
        }

        /// <summary>
        /// Преобразует результат Identity с ошибками в ответ валидации на русском языке.
        /// </summary>
        /// <param name="result">Результат операции Identity.</param>
        /// <returns>Объект ответа с общим сообщением и списком ошибок.</returns>
        private static ValidationErrorResponse ToValidationError(IdentityResult result)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return new ValidationErrorResponse(string.Join(" ", errors), errors);
        }
    }
}
