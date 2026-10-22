using Microsoft.AspNetCore.Mvc;
using PotionCraft.Contracts.Identity;
using PotionCraft.Server.Controllers;
using PotionCraft.Tests.Infrastructure;

namespace PotionCraft.Tests.Api;

/// <summary>
/// Тесты для UsersController (список пользователей и смена роли).
/// Используют реальные Identity-менеджеры поверх SQLite in-memory.
/// </summary>
public class UsersControllerTests : IDisposable
{
    /// <summary>Тестовое окружение Identity.</summary>
    private readonly IdentityTestHost _host;

    /// <summary>Тестируемый контроллер.</summary>
    private readonly UsersController _controller;

    /// <summary>Идентификатор администратора, от имени которого выполняются запросы.</summary>
    private readonly Guid _adminId;

    /// <summary>
    /// Инициализирует тестовое окружение, администратора и контроллер.
    /// </summary>
    public UsersControllerTests()
    {
        _host = new IdentityTestHost();
        var admin = _host.CreateUserAsync("admin", "admin@example.com", "password123", UserRoles.Admin).GetAwaiter().GetResult();
        _adminId = admin.Id;

        _controller = new UsersController(_host.UserManager)
        {
            ControllerContext = _host.CreateControllerContext(IdentityTestHost.CreatePrincipal(_adminId, UserRoles.Admin))
        };
    }

    /// <summary>
    /// Освобождает тестовое окружение.
    /// </summary>
    public void Dispose()
    {
        _host.Dispose();
    }

    // ─── GetAll ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, что список пользователей отсортирован по имени и содержит роли.
    /// </summary>
    [Fact]
    public async Task GetAll_ReturnsUsersSortedByUserNameWithRoles()
    {
        await _host.CreateUserAsync("zed", "zed@example.com", "password123", UserRoles.Player);
        await _host.CreateUserAsync("bob", "bob@example.com", "password123", UserRoles.Master);

        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        var users = Assert.IsAssignableFrom<IEnumerable<UserSummary>>(ok.Value).ToList();
        Assert.Equal(new[] { "admin", "bob", "zed" }, users.Select(u => u.UserName).ToArray());
        Assert.Equal(new[] { UserRoles.Master }, users.Single(u => u.UserName == "bob").Roles);
    }

    // ─── SetRole ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, что смена роли заменяет все роли пользователя единственной новой ролью.
    /// </summary>
    [Fact]
    public async Task SetRole_ValidRole_ReplacesRolesAndReturnsSummary()
    {
        var user = await _host.CreateUserAsync("hero", "hero@example.com", "password123", UserRoles.Player);

        var result = await _controller.SetRole(user.Id, new SetUserRoleRequest(UserRoles.Master));

        var ok = Assert.IsType<OkObjectResult>(result);
        var summary = Assert.IsType<UserSummary>(ok.Value);
        Assert.Equal(new[] { UserRoles.Master }, summary.Roles);
        Assert.Equal(new[] { UserRoles.Master }, await _host.UserManager.GetRolesAsync(user));
    }

    /// <summary>
    /// Проверяет, что недопустимая роль возвращает 400.
    /// </summary>
    [Fact]
    public async Task SetRole_InvalidRole_Returns400()
    {
        var user = await _host.CreateUserAsync("hero", "hero@example.com", "password123", UserRoles.Player);

        var result = await _controller.SetRole(user.Id, new SetUserRoleRequest("Wizard"));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    /// <summary>
    /// Проверяет, что смена роли несуществующего пользователя возвращает 404.
    /// </summary>
    [Fact]
    public async Task SetRole_UnknownUser_Returns404()
    {
        var result = await _controller.SetRole(Guid.NewGuid(), new SetUserRoleRequest(UserRoles.Master));

        Assert.IsType<NotFoundObjectResult>(result);
    }

    /// <summary>
    /// Проверяет, что администратор не может снять с себя роль Admin.
    /// </summary>
    [Fact]
    public async Task SetRole_AdminDemotesSelf_Returns400()
    {
        var result = await _controller.SetRole(_adminId, new SetUserRoleRequest(UserRoles.Player));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Нельзя снять с себя роль администратора.", badRequest.Value?.ToString());
        Assert.Equal(new[] { UserRoles.Admin }, await _host.UserManager.GetRolesAsync(await _host.UserManager.FindByIdAsync(_adminId.ToString()) ?? throw new InvalidOperationException()));
    }
}
