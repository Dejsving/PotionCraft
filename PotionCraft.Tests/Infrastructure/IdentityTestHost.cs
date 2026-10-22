using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PotionCraft.Contracts.Identity;
using PotionCraft.Repository;
using PotionCraft.Server.Services.Email;
using PotionCraft.Server.Services.Identity;

namespace PotionCraft.Tests.Infrastructure;

/// <summary>
/// Тестовое окружение ASP.NET Core Identity: реальные UserManager, RoleManager и SignInManager
/// поверх SQLite in-memory с теми же настройками, что и в приложении.
/// </summary>
public sealed class IdentityTestHost : IDisposable
{
    /// <summary>Соединение SQLite in-memory, которое должно оставаться открытым на время теста.</summary>
    private readonly SqliteConnection _connection;

    /// <summary>Корневой провайдер сервисов.</summary>
    private readonly ServiceProvider _rootProvider;

    /// <summary>Область видимости сервисов, используемая тестом.</summary>
    private readonly IServiceScope _scope;

    /// <summary>
    /// Создаёт тестовое окружение Identity с базой данных и заданной конфигурацией.
    /// </summary>
    /// <param name="configValues">Значения конфигурации (например, секция AdminSeed).</param>
    /// <param name="createRoles">Создавать ли все роли приложения сразу (как это делает IdentitySeeder при запуске).</param>
    public IdentityTestHost(IDictionary<string, string?>? configValues = null, bool createRoles = true)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues ?? new Dictionary<string, string?>())
            .Build();

        Clock = new ManualTimeProvider(new DateTimeOffset(2026, 10, 22, 12, 0, 0, TimeSpan.Zero));
        EmailSender = new FakeEmailSender();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddHttpContextAccessor();
        services.AddDbContext<PotionCraftDbContext>(options => options.UseSqlite(_connection));
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(IdentityConfiguration.ConfigureIdentityOptions)
            .AddEntityFrameworkStores<PotionCraftDbContext>()
            .AddDefaultTokenProviders()
            .AddErrorDescriber<RussianIdentityErrorDescriber>();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddScoped<IdentitySeeder>();
        services.AddSingleton<TimeProvider>(Clock);
        services.AddSingleton<IEmailSender>(EmailSender);
        services.AddScoped<IEmailConfirmationService, EmailConfirmationService>();

        _rootProvider = services.BuildServiceProvider();
        _scope = _rootProvider.CreateScope();
        Services.GetRequiredService<PotionCraftDbContext>().Database.EnsureCreated();

        if (createRoles)
        {
            foreach (var role in UserRoles.All)
                EnsureRoleAsync(role).GetAwaiter().GetResult();
        }
    }

    /// <summary>Провайдер сервисов тестовой области видимости.</summary>
    public IServiceProvider Services => _scope.ServiceProvider;

    /// <summary>Управляемые часы, которыми сервисы получают текущее время.</summary>
    public ManualTimeProvider Clock { get; }

    /// <summary>Фиктивный отправитель писем, сохраняющий отправленные письма.</summary>
    public FakeEmailSender EmailSender { get; }

    /// <summary>Сервис подтверждения почты.</summary>
    public IEmailConfirmationService EmailConfirmation => Services.GetRequiredService<IEmailConfirmationService>();

    /// <summary>Менеджер пользователей.</summary>
    public UserManager<ApplicationUser> UserManager => Services.GetRequiredService<UserManager<ApplicationUser>>();

    /// <summary>Менеджер ролей.</summary>
    public RoleManager<IdentityRole<Guid>> RoleManager => Services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    /// <summary>Менеджер входа в систему.</summary>
    public SignInManager<ApplicationUser> SignInManager => Services.GetRequiredService<SignInManager<ApplicationUser>>();

    /// <summary>Инициализатор ролей и администратора.</summary>
    public IdentitySeeder Seeder => Services.GetRequiredService<IdentitySeeder>();

    /// <summary>
    /// Создаёт HttpContext с указанным пользователем и регистрирует его в IHttpContextAccessor,
    /// чтобы SignInManager записывал cookie в ответ.
    /// </summary>
    /// <param name="user">Текущий пользователь запроса; по умолчанию анонимный.</param>
    /// <returns>Контекст HTTP-запроса.</returns>
    public DefaultHttpContext CreateHttpContext(ClaimsPrincipal? user = null)
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = Services,
            User = user ?? new ClaimsPrincipal(new ClaimsIdentity())
        };
        Services.GetRequiredService<IHttpContextAccessor>().HttpContext = httpContext;
        return httpContext;
    }

    /// <summary>
    /// Создаёт ControllerContext с HTTP-контекстом для указанного пользователя.
    /// </summary>
    /// <param name="user">Текущий пользователь запроса; по умолчанию анонимный.</param>
    /// <returns>Контекст контроллера.</returns>
    public ControllerContext CreateControllerContext(ClaimsPrincipal? user = null)
    {
        return new ControllerContext { HttpContext = CreateHttpContext(user) };
    }

    /// <summary>
    /// Создаёт пользователя с паролем и ролью.
    /// </summary>
    /// <param name="userName">Имя пользователя.</param>
    /// <param name="email">Адрес электронной почты.</param>
    /// <param name="password">Пароль.</param>
    /// <param name="role">Роль, назначаемая пользователю (необязательно).</param>
    /// <param name="emailConfirmed">Подтверждена ли почта пользователя (по умолчанию да, чтобы пользователь мог войти).</param>
    /// <returns>Созданный пользователь.</returns>
    public async Task<ApplicationUser> CreateUserAsync(string userName, string email, string password, string? role = null,
        bool emailConfirmed = true)
    {
        var user = new ApplicationUser { UserName = userName, Email = email, EmailConfirmed = emailConfirmed };
        var createResult = await UserManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            throw new InvalidOperationException(string.Join("; ", createResult.Errors.Select(e => e.Description)));

        if (role != null)
        {
            await EnsureRoleAsync(role);
            await UserManager.AddToRoleAsync(user, role);
        }

        return user;
    }

    /// <summary>
    /// Создаёт роль, если её ещё нет в базе данных.
    /// </summary>
    /// <param name="role">Название роли.</param>
    public async Task EnsureRoleAsync(string role)
    {
        if (!await RoleManager.RoleExistsAsync(role))
            await RoleManager.CreateAsync(new IdentityRole<Guid>(role));
    }

    /// <summary>
    /// Создаёт ClaimsPrincipal с идентификатором пользователя и ролями.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="roles">Роли пользователя.</param>
    /// <returns>Аутентифицированный ClaimsPrincipal.</returns>
    public static ClaimsPrincipal CreatePrincipal(Guid userId, params string[] roles)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    /// <summary>
    /// Освобождает область видимости, провайдер и соединение с базой данных.
    /// </summary>
    public void Dispose()
    {
        _scope.Dispose();
        _rootProvider.Dispose();
        _connection.Dispose();
    }
}
