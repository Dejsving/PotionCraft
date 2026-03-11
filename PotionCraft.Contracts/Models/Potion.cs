using PotionCraft.Contracts.Enums;

namespace PotionCraft.Contracts.Models;

/// <summary>
/// Готовое зелье — результат успешной варки по рецепту.
/// </summary>
public class Potion
{
    /// <summary>
    /// Уникальный идентификатор зелья.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Название зелья.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание эффекта зелья.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Тип зелья.
    /// </summary>
    public PotionTypeEnum Type { get; set; }

    /// <summary>
    /// Рецепт, по которому было сварено зелье.
    /// </summary>
    public required AlchemyRecipe Recipe { get; set; }

    /// <summary>
    /// Продолжительность действия в раундах (0 = мгновенное).
    /// </summary>
    public int DurationRounds { get; set; }
}