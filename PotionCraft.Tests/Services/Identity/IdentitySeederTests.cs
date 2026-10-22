using PotionCraft.Contracts.Identity;
using PotionCraft.Tests.Infrastructure;

namespace PotionCraft.Tests.Services.Identity;

/// <summary>
/// Тесты для IdentitySeeder: создание ролей и первого администратора из конфигурации.
/// </summary>
public class IdentitySeederTests : IDisposable
{
    /// <summary>Конфигурация с корректной секцией AdminSeed.</summary>
    private static readonly Dictionary<string, string?> ValidAdminConfig = new()
    {
        ["AdminSeed:UserName"] = "root",
        ["AdminSeed:Email"] = "root@example.com",
        ["AdminSeed:Password"] = "rootpassword"
    };

    /// <summary>Тестовое окружение, созданное внутри каждого теста.</summary>
    private IdentityTestHost? _host;

    /// <summary>
    /// Освобождает тестовое окружение, если оно было создано.
    /// </summary>
    public void Dispose()
    {
        _host?.Dispose();
    }

    /// <summary>
    /// Проверяет, что все роли из UserRoles создаются при инициализации.
    /// </summary>
    [Fact]
    public async Task SeedAsync_CreatesAllRoles()
    {
        _host = new IdentityTestHost(createRoles: false);

        await _host.Seeder.SeedAsync();

        foreach (var role in UserRoles.All)
            Assert.True(await _host.RoleManager.RoleExistsAsync(role), $"Роль {role} не создана.");
    }

    /// <summary>
    /// Проверяет, что повторная инициализация не создаёт дубликатов ролей.
    /// </summary>
    [Fact]
    public async Task SeedAsync_CalledTwice_DoesNotDuplicateRoles()
    {
        _host = new IdentityTestHost(createRoles: false);

        await _host.Seeder.SeedAsync();
        await _host.Seeder.SeedAsync();

        Assert.Equal(UserRoles.All.Count, _host.RoleManager.Roles.Count());
    }

    /// <summary>
    /// Проверяет, что администратор создаётся из конфигурации с ролью Admin.
    /// </summary>
    [Fact]
    public async Task SeedAsync_WithAdminConfig_CreatesAdminWithAdminRole()
    {
        _host = new IdentityTestHost(ValidAdminConfig, createRoles: false);

        await _host.Seeder.SeedAsync();

        var admin = await _host.UserManager.FindByNameAsync("root");
        Assert.NotNull(admin);
        Assert.Equal("root@example.com", admin.Email);
        Assert.Equal(new[] { UserRoles.Admin }, await _host.UserManager.GetRolesAsync(admin));
        Assert.True(await _host.UserManager.CheckPasswordAsync(admin, "rootpassword"));
    }

    /// <summary>
    /// Проверяет, что без пароля администратор не создаётся, а инициализация не падает.
    /// </summary>
    [Fact]
    public async Task SeedAsync_WithoutPassword_SkipsAdminCreation()
    {
        _host = new IdentityTestHost(new Dictionary<string, string?>
        {
            ["AdminSeed:UserName"] = "root",
            ["AdminSeed:Email"] = "root@example.com",
            ["AdminSeed:Password"] = ""
        }, createRoles: false);

        await _host.Seeder.SeedAsync();

        Assert.Null(await _host.UserManager.FindByNameAsync("root"));
        Assert.Equal(UserRoles.All.Count, _host.RoleManager.Roles.Count());
    }

    /// <summary>
    /// Проверяет, что при отсутствии секции AdminSeed администратор не создаётся.
    /// </summary>
    [Fact]
    public async Task SeedAsync_WithoutSection_SkipsAdminCreation()
    {
        _host = new IdentityTestHost(createRoles: false);

        await _host.Seeder.SeedAsync();

        Assert.Empty(_host.UserManager.Users);
    }

    /// <summary>
    /// Проверяет, что созданный из конфигурации администратор сразу имеет подтверждённую почту.
    /// </summary>
    [Fact]
    public async Task SeedAsync_CreatedAdmin_HasConfirmedEmail()
    {
        _host = new IdentityTestHost(ValidAdminConfig, createRoles: false);

        await _host.Seeder.SeedAsync();

        var admin = await _host.UserManager.FindByNameAsync("root");
        Assert.NotNull(admin);
        Assert.True(admin.EmailConfirmed);
    }

    /// <summary>
    /// Проверяет, что существующий администратор с неподтверждённой почтой получает подтверждение.
    /// </summary>
    [Fact]
    public async Task SeedAsync_ExistingUnconfirmedAdmin_BecomesConfirmed()
    {
        _host = new IdentityTestHost(ValidAdminConfig, createRoles: false);
        await _host.CreateUserAsync("root", "root@example.com", "otherpassword", emailConfirmed: false);

        await _host.Seeder.SeedAsync();

        var admin = await _host.UserManager.FindByNameAsync("root");
        Assert.NotNull(admin);
        Assert.True(admin.EmailConfirmed);
    }

    /// <summary>
    /// Проверяет, что существующий пользователь получает роль Admin и теряет остальные роли.
    /// </summary>
    [Fact]
    public async Task SeedAsync_ExistingUser_GetsOnlyAdminRole()
    {
        _host = new IdentityTestHost(ValidAdminConfig, createRoles: false);
        var existing = await _host.CreateUserAsync("root", "root@example.com", "otherpassword", UserRoles.Player);
        await _host.EnsureRoleAsync(UserRoles.Master);
        await _host.UserManager.AddToRoleAsync(existing, UserRoles.Master);

        await _host.Seeder.SeedAsync();

        Assert.Equal(new[] { UserRoles.Admin }, await _host.UserManager.GetRolesAsync(existing));
        Assert.Single(_host.UserManager.Users);
    }
}
