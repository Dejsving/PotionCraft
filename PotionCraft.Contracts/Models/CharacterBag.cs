namespace PotionCraft.Contracts.Models;

/// <summary>
/// Сумка персонажа — хранит собранные травы, созданные зелья и яды.
/// </summary>
public class CharacterBag
{
    /// <summary>
    /// Уникальный идентификатор сумки.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Собранные травы. Ключ — идентификатор травы, значение — результат сбора с количеством.
    /// </summary>
    public Dictionary<Guid, GatheringResult> Herbs { get; set; } = new();

    /// <summary>
    /// Созданные зелья. Ключ — идентификатор зелья, значение — зелье с количеством.
    /// </summary>
    public Dictionary<Guid, PotionBagItem> Potions { get; set; } = new();

    /// <summary>
    /// Созданные яды. Ключ — идентификатор яда, значение — яд с количеством.
    /// </summary>
    public Dictionary<Guid, PotionBagItem> Poisons { get; set; } = new();
}
