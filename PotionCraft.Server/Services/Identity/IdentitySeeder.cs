using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PotionCraft.Contracts.Identity;

namespace PotionCraft.Server.Services.Identity;

/// <summary>
/// Инициализирует роли и первого администратора при запуске приложения.
/// </summary>
public class IdentitySeeder
{
    /// <summary>Менеджер ролей.</summary>
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    /// <summary>Менеджер пользователей.</summary>
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>Конфигурация приложения (секция AdminSeed).</summary>
    private readonly IConfiguration _configuration;

    /// <summary>Логгер.</summary>
    private readonly ILogger<IdentitySeeder> _logger;

    /// <summary>
    /// Создаёт инициализатор Identity.
    /// </summary>
    /// <param name="roleManager">Менеджер ролей.</param>
    /// <param name="userManager">Менеджер пользователей.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <param name="logger">Логгер.</param>
    public IdentitySeeder(RoleManager<IdentityRole<Guid>> roleManager, UserManager<ApplicationUser> userManager,
        IConfiguration configuration, ILogger<IdentitySeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Создаёт недостающие роли и гарантирует наличие администратора из конфигурации.
    /// Если секция AdminSeed не задана или пароль не указан, администратор не создаётся.
    /// </summary>
    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedAdminAsync();
    }

    /// <summary>
    /// Создаёт роли, которых ещё нет в базе данных.
    /// </summary>
    private async Task SeedRolesAsync()
    {
        foreach (var role in UserRoles.All)
        {
            if (await _roleManager.RoleExistsAsync(role))
                continue;

            var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
            if (!result.Succeeded)
                _logger.LogError("Не удалось создать роль {Role}: {Errors}", role,
                    string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    /// <summary>
    /// Создаёт администратора из секции AdminSeed либо назначает роль Admin существующему пользователю.
    /// У администратора остаётся только роль Admin.
    /// </summary>
    private async Task SeedAdminAsync()
    {
        var section = _configuration.GetSection("AdminSeed");
        var userName = section["UserName"];
        var email = section["Email"];
        var password = section["Password"];

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Секция AdminSeed не задана или не содержит UserName, Email и Password. Администратор не создан.");
            return;
        }

        var admin = await _userManager.FindByNameAsync(userName) ?? await _userManager.FindByEmailAsync(email);
        if (admin == null)
        {
            admin = new ApplicationUser { UserName = userName, Email = email, EmailConfirmed = true };
            var createResult = await _userManager.CreateAsync(admin, password);
            if (!createResult.Succeeded)
            {
                _logger.LogError("Не удалось создать администратора {UserName}: {Errors}", userName,
                    string.Join("; ", createResult.Errors.Select(e => e.Description)));
                return;
            }
        }
        else if (!admin.EmailConfirmed)
        {
            // Администратор из конфигурации не должен застревать на подтверждении почты.
            admin.EmailConfirmed = true;
            await _userManager.UpdateAsync(admin);
        }

        var currentRoles = await _userManager.GetRolesAsync(admin);
        var rolesToRemove = currentRoles.Where(r => r != UserRoles.Admin).ToList();
        if (rolesToRemove.Count > 0)
            await _userManager.RemoveFromRolesAsync(admin, rolesToRemove);

        if (!currentRoles.Contains(UserRoles.Admin))
            await _userManager.AddToRoleAsync(admin, UserRoles.Admin);
    }
}
