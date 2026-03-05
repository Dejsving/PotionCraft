using PotionCraft.Contracts.Enums;

namespace PotionCraft.Contracts.Models;

/// <summary>
/// Описывает эффект отдельной части растения при использовании в алхимии.
/// </summary>
public class HerbComponent
{
    /// <summary>Часть растения.</summary>
    //public PlantPart Part { get; set; }

    /// <summary>Тип эффекта этой части.</summary>
    //public ComponentEffectType EffectType { get; set; }

    /// <summary>Описание эффекта в свободной форме.</summary>
    public string EffectDescription { get; set; } = string.Empty;

    /// <summary>
    /// Потенция компонента — модификатор, добавляемый к броску при варке.
    /// </summary>
    public int Potency { get; set; }

    /// <summary>
    /// Необходимый инструмент для обработки компонента
    /// (например, ступка и пестик, перегонный куб и т.д.).
    /// </summary>
    public string? RequiredTool { get; set; }
}
