namespace PotionCraft.Contracts.Identity;

/// <summary>
/// Названия ролей пользователей приложения.
/// </summary>
public static class UserRoles
{
    /// <summary>Администратор приложения.</summary>
    public const string Admin = "Admin";

    /// <summary>Мастер игры.</summary>
    public const string Master = "Master";

    /// <summary>Игрок (роль по умолчанию для новых пользователей).</summary>
    public const string Player = "Player";

    /// <summary>Все допустимые роли.</summary>
    public static readonly IReadOnlyList<string> All = [Admin, Master, Player];
}
