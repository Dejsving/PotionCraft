using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PotionCraft.Contracts.Identity;
using PotionCraft.Server.Extensions;

namespace PotionCraft.Server.Controllers
{
    /// <summary>
    /// Краткие данные пользователя для API (включая текущего пользователя).
    /// </summary>
    /// <param name="Id">Идентификатор пользователя.</param>
    /// <param name="UserName">Имя пользователя.</param>
    /// <param name="Email">Адрес электронной почты.</param>
    /// <param name="Roles">Роли пользователя.</param>
    public record UserSummary(Guid Id, string UserName, string Email, IReadOnlyList<string> Roles)
    {
        /// <summary>
        /// Создаёт краткое описание пользователя.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <param name="roles">Роли пользователя.</param>
        /// <returns>Краткое описание пользователя.</returns>
        public static UserSummary FromUser(ApplicationUser user, IEnumerable<string> roles)
            => new(user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty, roles.ToList());
    }

    /// <summary>
    /// Запрос на смену роли пользователя.
    /// </summary>
    /// <param name="Role">Новая роль (Admin, Master или Player).</param>
    public record SetUserRoleRequest(string Role);

    /// <summary>
    /// Управление пользователями (только для администраторов).
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = UserRoles.Admin)]
    public class UsersController : ControllerBase
    {
        /// <summary>Менеджер пользователей.</summary>
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Создаёт контроллер пользователей.
        /// </summary>
        /// <param name="userManager">Менеджер пользователей.</param>
        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Возвращает всех пользователей, отсортированных по имени.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.UserName)
                .ToListAsync();

            var result = new List<UserSummary>(users.Count);
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(UserSummary.FromUser(user, roles));
            }

            return Ok(result);
        }

        /// <summary>
        /// Заменяет все роли пользователя единственной указанной ролью.
        /// Администратор не может снять с себя роль Admin.
        /// </summary>
        /// <param name="id">Идентификатор пользователя.</param>
        /// <param name="request">Новая роль.</param>
        [HttpPut("{id:guid}/role")]
        public async Task<IActionResult> SetRole(Guid id, [FromBody] SetUserRoleRequest request)
        {
            if (!UserRoles.All.Contains(request.Role))
                return BadRequest(new { message = "Указана недопустимая роль." });

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound(new { message = "Пользователь не найден." });

            if (User.GetUserId() == id && request.Role != UserRoles.Admin)
                return BadRequest(new { message = "Нельзя снять с себя роль администратора." });

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                    return BadRequest(new { message = string.Join(" ", removeResult.Errors.Select(e => e.Description)) });
            }

            var addResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!addResult.Succeeded)
                return BadRequest(new { message = string.Join(" ", addResult.Errors.Select(e => e.Description)) });

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(UserSummary.FromUser(user, roles));
        }
    }
}
