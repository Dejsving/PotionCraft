using PotionCraft.Contracts.Enums;

namespace PotionCraft.Contracts.Models;

/// <summary>
/// Результат попытки сварить зелье по рецепту.
/// </summary>
public class BrewingResult
{
    /// <summary>Рецепт, по которому варилось зелье.</summary>
    public required AlchemyRecipe Recipe { get; set; }

    /// <summary>Значение броска d20.</summary>
    public int RollResult { get; set; }

    /// <summary>Модификатор персонажа (Интеллект + владение инструментами алхимика).</summary>
    public int Modifier { get; set; }

    /// <summary>Итоговое значение проверки.</summary>
    public int Total => RollResult + Modifier;

    /// <summary>Исход варки.</summary>
    public RollResultEnum Outcome { get; set; }

    /// <summary>Сваренное зелье (null при провале).</summary>
    public Potion? ResultingPotion { get; set; }

    /// <summary>Описание того, что произошло в процессе варки.</summary>
    public string NarrativeDescription { get; set; } = string.Empty;

    /// <summary>
    /// Количество доз в результате: при критическом успехе удваивается,
    /// при критическом провале — 0.
    /// </summary>
    public int DosesProduced { get; set; }
}
