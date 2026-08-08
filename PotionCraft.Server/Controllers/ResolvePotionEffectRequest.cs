namespace PotionCraft.Server.Controllers;

/// <summary>
/// Запрос на вычисление итогового эффекта зелья по травам.
/// </summary>
public class ResolvePotionEffectRequest
{
    /// <summary>
    /// Идентификатор травы-основы.
    /// </summary>
    public Guid BaseHerbId { get; set; }

    /// <summary>
    /// Идентификаторы трав-модификаторов в порядке их добавления игроком.
    /// </summary>
    public List<Guid> ModifierHerbIds { get; set; } = [];
}
