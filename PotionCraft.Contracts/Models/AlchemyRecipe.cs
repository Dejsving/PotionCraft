using PotionCraft.Contracts.Enums;

namespace PotionCraft.Contracts.Models;

/// <summary>
/// Рецепт алхимического зелья.
/// </summary>
public class AlchemyRecipe
{
    /// <summary>Уникальный идентификатор рецепта.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Название рецепта.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Описание результирующего зелья.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Тип получаемого зелья.</summary>
    public PotionType PotionType { get; set; }

    /// <summary>
    /// Сложность (DC) проверки инструментов алхимика для варки.
    /// </summary>
    public int BrewingDC { get; set; }

    /// <summary>
    /// Список необходимых компонентов: ключ — трава, значение — часть растения.
    /// </summary>
    public IReadOnlyList<RecipeIngredient> Ingredients { get; set; } = [];

    /// <summary>Время приготовления в часах.</summary>
    public int BrewingTimeHours { get; set; }

    /// <summary>Количество получаемых доз при успешной варке.</summary>
    public int DosesProduced { get; set; } = 1;
}
