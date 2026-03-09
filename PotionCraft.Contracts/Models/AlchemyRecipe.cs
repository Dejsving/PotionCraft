using PotionCraft.Contracts.Enums;

namespace PotionCraft.Contracts.Models;

/// <summary>
/// Рецепт алхимического зелья.
/// </summary>
public class AlchemyRecipe
{
    /// <summary>
    /// Уникальный идентификатор рецепта.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Название рецепта.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание зелья.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Тип зелья.
    /// </summary>
    public PotionTypeEnum PotionType { get; set; }

    /// <summary>
    /// Сложность (DC) проверки инструментов алхимика для варки.
    /// </summary>
    public int BrewingDC { get; set; } = 10;

    /// <summary>
    /// Список необходимых растений
    /// </summary>
    public IReadOnlyList<Herb> Ingredients { get; set; } = [];

    /// <summary>
    /// 
    /// </summary>
    public List<string> Modifications { get; set; } = new();
}