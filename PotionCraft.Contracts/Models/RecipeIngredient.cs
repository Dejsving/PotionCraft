namespace PotionCraft.Contracts.Models;

/// <summary>
/// Компонент рецепта: конкретная часть конкретной травы.
/// </summary>
public class RecipeIngredient
{
    /// <summary>Трава-ингредиент.</summary>
    public required Herb Herb { get; set; }

    /// <summary>Используемый компонент травы.</summary>
    public required HerbComponent Component { get; set; }

    /// <summary>Необходимое количество доз.</summary>
    public int Quantity { get; set; } = 1;
}
