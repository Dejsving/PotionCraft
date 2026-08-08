using PotionCraft.Contracts.Enums;

namespace PotionCraft.Contracts.Models;

/// <summary>
/// Результат применения модификаторов к базовому эффекту травы для отображения на странице «Алхимия».
/// </summary>
public class PotionEffectResult
{
    /// <summary>
    /// Итоговое количество костей. Актуально только если <see cref="IsFixedEffect"/> равен false.
    /// </summary>
    public int DiceCount { get; set; }

    /// <summary>
    /// Итоговый тип кости. Актуален только если <see cref="IsFixedEffect"/> равен false.
    /// </summary>
    public DiceTypeEnum? DiceType { get; set; }

    /// <summary>
    /// Прибавляется ли к результату броска Мод. Алхимии.
    /// </summary>
    public bool IncludesAlchemyMod { get; set; }

    /// <summary>
    /// Итоговый тип урона (если применимо).
    /// </summary>
    public DamageTypeEnum? DamageType { get; set; }

    /// <summary>
    /// Варианты типов урона на выбор, если один из модификаторов предлагает замену типа урона.
    /// </summary>
    public IReadOnlyList<DamageTypeEnum> DamageTypeChoices { get; set; } = [];

    /// <summary>
    /// Текстовые заметки о поведенческих и прочих не вычисляемых числом эффектах модификаторов, в порядке применения.
    /// </summary>
    public IReadOnlyList<string> Notes { get; set; } = [];

    /// <summary>
    /// Заметка о том, что часть итогового эффекта отдана на решение Мастера (например, из-за Хромовой слизи). Null, если такого модификатора нет.
    /// </summary>
    public string? DmJudgementNote { get; set; }

    /// <summary>
    /// true, если у базовой травы нет вычисляемой формулы и итоговое описание — это просто последовательность исходных текстов Effect.
    /// </summary>
    public bool IsFixedEffect { get; set; }

    /// <summary>
    /// Готовая строка вычисленной формулы (префикс + кости + Мод. Алхимии + тип урона), без заметок и решения Мастера. Пусто для фиксированного эффекта.
    /// </summary>
    public string FormulaText { get; set; } = string.Empty;

    /// <summary>
    /// Готовое итоговое текстовое описание эффекта зелья для отображения в UI.
    /// </summary>
    public string GeneratedDescription { get; set; } = string.Empty;
}
