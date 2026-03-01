using PotionCraft.Contracts.Enums;

namespace PotionCraft.Contracts.Models;

/// <summary>
/// Бросок проверки навыка гербализма при поиске или идентификации растений.
/// </summary>
public class HerbalistCheck
{
    /// <summary>Значение броска d20.</summary>
    public int RollResult { get; set; }

    /// <summary>Модификатор персонажа (Мудрость + владение инструментами травника).</summary>
    public int Modifier { get; set; }

    /// <summary>Итоговое значение проверки (бросок + модификатор).</summary>
    public int Total => RollResult + Modifier;

    /// <summary>Трава, которую персонаж пытается найти или идентифицировать.</summary>
    public required Herb TargetHerb { get; set; }

    /// <summary>Преодолена ли сложность (DC) проверки.</summary>
    public bool IsSuccess => Total >= TargetHerb.FindDC;

    /// <summary>
    /// Критический успех: Total >= DC + 10. Персонаж находит двойное количество.
    /// </summary>
    public bool IsCriticalSuccess => Total >= TargetHerb.FindDC + 10;
}
