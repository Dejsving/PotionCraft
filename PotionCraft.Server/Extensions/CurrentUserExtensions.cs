using System.Security.Claims;

namespace PotionCraft.Server.Extensions
{
    /// <summary>
    /// Методы расширения для получения данных текущего пользователя из <see cref="ClaimsPrincipal"/>.
    /// </summary>
    public static class CurrentUserExtensions
    {
        /// <summary>
        /// Возвращает идентификатор текущего пользователя из claim NameIdentifier.
        /// </summary>
        /// <param name="principal">Текущий пользователь.</param>
        /// <returns>Идентификатор пользователя или <c>null</c>, если claim отсутствует или некорректен.</returns>
        public static Guid? GetUserId(this ClaimsPrincipal principal)
        {
            var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
